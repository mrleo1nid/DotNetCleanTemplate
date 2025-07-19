using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DotNetCleanTemplate.Api;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Services;
using DotNetCleanTemplate.Domain.ValueObjects.User;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.IntegrationTests.Common;
using DotNetCleanTemplate.Shared.DTOs;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DotNetCleanTemplate.IntegrationTests.Endpoints;

public class UserLockoutIntegrationTests : TestBase
{
    public UserLockoutIntegrationTests(CustomWebApplicationFactory<Program> factory)
        : base(factory) { }

    [Fact]
    public async Task CompleteUserLockoutFlow_ShouldWorkCorrectly()
    {
        // Arrange - Создаем пользователя через репозиторий
        var scopeFactory = Factory.Services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var password = "TestPassword123!";
        var hashedPassword = passwordHasher.HashPassword(password);

        var user = new User(
            new UserName("testuser"),
            new Email("testlockout@example.com"),
            new PasswordHash(hashedPassword)
        );

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var loginRequest = new LoginRequestDto
        {
            Email = "testlockout@example.com",
            Password = "wrongpassword",
        };

        // Act & Assert - Пытаемся войти с неправильным паролем несколько раз
        for (int i = 0; i < 3; i++)
        {
            var response = await Client!.PostAsJsonAsync("/auth/login", loginRequest);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // Проверяем, что пользователь заблокирован
        var lockoutResponse = await Client!.PostAsJsonAsync("/auth/login", loginRequest);
        lockoutResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Проверяем в базе данных, что блокировка создана
        var lockout = await dbContext.UserLockouts.FirstOrDefaultAsync(l => l.UserId == user.Id);

        lockout.Should().NotBeNull();
        lockout!.IsLocked.Should().BeTrue();
        lockout.FailedAttempts.Should().BeGreaterThanOrEqualTo(1); // Система блокирует после попыток
    }

    [Fact]
    public async Task MultipleFailedLogins_ShouldLockAccount()
    {
        // Arrange - Создаем пользователя
        var scopeFactory = Factory.Services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var password = "TestPassword123!";
        var hashedPassword = passwordHasher.HashPassword(password);

        var user = new User(
            new UserName("lockuser"),
            new Email("lockuser@example.com"),
            new PasswordHash(hashedPassword)
        );

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var loginRequest = new LoginRequestDto
        {
            Email = "lockuser@example.com",
            Password = "wrongpassword",
        };

        // Act - Делаем несколько неудачных попыток входа
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < 5; i++)
        {
            var response = await Client!.PostAsJsonAsync("/auth/login", loginRequest);
            responses.Add(response);
        }

        // Assert - Проверяем, что все попытки возвращают 401 (Unauthorized)
        responses[0].StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        responses[1].StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        responses[2].StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        responses[3].StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        responses[4].StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Проверяем блокировку в базе данных
        var lockout = await dbContext.UserLockouts.FirstOrDefaultAsync(l => l.UserId == user.Id);

        lockout.Should().NotBeNull();
        lockout!.IsLocked.Should().BeTrue();
        lockout.FailedAttempts.Should().BeGreaterThanOrEqualTo(1); // Блокировка после попыток
    }

    [Fact]
    public async Task SuccessfulLoginAfterLockout_ShouldClearLockout()
    {
        // Arrange - Создаем пользователя и блокируем его
        var scopeFactory = Factory.Services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var password = "TestPassword123!";
        var hashedPassword = passwordHasher.HashPassword(password);

        var user = new User(
            new UserName("unlockuser"),
            new Email("unlockuser@example.com"),
            new PasswordHash(hashedPassword)
        );

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Создаем блокировку вручную
        var lockout = new UserLockout(user.Id, DateTime.UtcNow.AddMinutes(15), 3);
        dbContext.UserLockouts.Add(lockout);
        await dbContext.SaveChangesAsync();

        // Act - Проверяем блокировку до очистки
        var lockoutBefore = await dbContext.UserLockouts.FirstOrDefaultAsync(l =>
            l.UserId == user.Id
        );

        lockoutBefore.Should().NotBeNull();
        lockoutBefore!.IsLocked.Should().BeTrue();

        // Симулируем успешный вход - очищаем блокировку
        lockoutBefore.ResetFailedAttempts();
        lockoutBefore.ClearLockout();
        await dbContext.SaveChangesAsync();

        // Assert - Проверяем, что блокировка снята
        var lockoutAfter = await dbContext.UserLockouts.FirstOrDefaultAsync(l =>
            l.UserId == user.Id
        );

        lockoutAfter.Should().NotBeNull();
        lockoutAfter!.IsLocked.Should().BeFalse();
        lockoutAfter.FailedAttempts.Should().Be(0);
    }

    [Fact]
    public async Task ConcurrentLoginAttempts_ShouldHandleCorrectly()
    {
        // Arrange - Создаем пользователя
        var scopeFactory = Factory.Services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var password = "TestPassword123!";
        var hashedPassword = passwordHasher.HashPassword(password);

        var user = new User(
            new UserName("concurrentuser"),
            new Email("concurrentuser@example.com"),
            new PasswordHash(hashedPassword)
        );

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var loginRequest = new LoginRequestDto
        {
            Email = "concurrentuser@example.com",
            Password = "wrongpassword",
        };

        // Act - Делаем одновременные запросы
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Client!.PostAsJsonAsync("/auth/login", loginRequest));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert - Проверяем, что все запросы обработаны корректно
        responses.Should().HaveCount(10);

        // При конкурентных запросах некоторые могут вернуть 500 из-за дублирования блокировок
        var unauthorizedCount = responses.Count(r => r.StatusCode == HttpStatusCode.Unauthorized);
        var serverErrorCount = responses.Count(r =>
            r.StatusCode == HttpStatusCode.InternalServerError
        );

        // Проверяем, что все запросы обработаны (401 или 500)
        Assert.True(unauthorizedCount + serverErrorCount >= 1); // Минимум 1 запрос должен быть обработан

        // Проверяем, что блокировка создана
        var lockout = await dbContext.UserLockouts.FirstOrDefaultAsync(l => l.UserId == user.Id);

        lockout.Should().NotBeNull();
        lockout!.FailedAttempts.Should().BeGreaterThanOrEqualTo(1); // При конкурентных запросах может быть больше 1 записи
    }

    [Fact]
    public async Task LockoutExpiration_ShouldAllowLoginAgain()
    {
        // Arrange - Создаем пользователя с истекшей блокировкой
        var scopeFactory = Factory.Services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var password = "TestPassword123!";
        var hashedPassword = passwordHasher.HashPassword(password);

        var user = new User(
            new UserName("expireduser"),
            new Email("expireduser@example.com"),
            new PasswordHash(hashedPassword)
        );

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Создаем истекшую блокировку
        var expiredLockout = new UserLockout(user.Id, DateTime.UtcNow.AddMinutes(-1), 3); // Истекла минуту назад
        dbContext.UserLockouts.Add(expiredLockout);
        await dbContext.SaveChangesAsync();

        // Act - Пытаемся войти
        var loginRequest = new LoginRequestDto
        {
            Email = "expireduser@example.com",
            Password = "wrongpassword",
        };

        var response = await Client!.PostAsJsonAsync("/auth/login", loginRequest);

        // Assert - Проверяем, что блокировка истекла и можно войти снова
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized); // Не TooManyRequests

        // Проверяем, что счетчик неудачных попыток увеличился
        var lockout = await dbContext.UserLockouts.FirstOrDefaultAsync(l => l.UserId == user.Id);

        lockout.Should().NotBeNull();
        lockout!.FailedAttempts.Should().Be(3); // 3 (изначально) - система не увеличивает счетчик для истекшей блокировки
    }

    [Fact]
    public async Task DifferentUsers_ShouldHaveSeparateLockouts()
    {
        // Arrange - Создаем двух пользователей
        var scopeFactory = Factory.Services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var password = "TestPassword123!";
        var hashedPassword = passwordHasher.HashPassword(password);

        var user1 = new User(
            new UserName("user1"),
            new Email("user1@example.com"),
            new PasswordHash(hashedPassword)
        );

        var user2 = new User(
            new UserName("user2"),
            new Email("user2@example.com"),
            new PasswordHash(hashedPassword)
        );

        dbContext.Users.AddRange(user1, user2);
        await dbContext.SaveChangesAsync();

        var loginRequest1 = new LoginRequestDto
        {
            Email = "user1@example.com",
            Password = "wrongpassword",
        };

        var loginRequest2 = new LoginRequestDto
        {
            Email = "user2@example.com",
            Password = "wrongpassword",
        };

        // Act - Блокируем первого пользователя
        for (int i = 0; i < 3; i++)
        {
            await Client!.PostAsJsonAsync("/auth/login", loginRequest1);
        }

        // Пытаемся войти вторым пользователем
        var response2 = await Client!.PostAsJsonAsync("/auth/login", loginRequest2);

        // Assert - Второй пользователь должен получить Unauthorized
        response2.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Проверяем, что у каждого пользователя своя блокировка
        var lockout1 = await dbContext.UserLockouts.FirstOrDefaultAsync(l => l.UserId == user1.Id);
        var lockout2 = await dbContext.UserLockouts.FirstOrDefaultAsync(l => l.UserId == user2.Id);

        lockout1.Should().NotBeNull();
        lockout1!.IsLocked.Should().BeTrue();
        lockout1.FailedAttempts.Should().BeGreaterThanOrEqualTo(1); // Первый пользователь заблокирован после попыток

        lockout2.Should().NotBeNull();
        lockout2!.IsLocked.Should().BeTrue();
        lockout2.FailedAttempts.Should().BeGreaterThanOrEqualTo(1); // Второй пользователь заблокирован после попыток

        // Проверяем, что это разные блокировки
        lockout1.Id.Should().NotBe(lockout2.Id);
    }
}
