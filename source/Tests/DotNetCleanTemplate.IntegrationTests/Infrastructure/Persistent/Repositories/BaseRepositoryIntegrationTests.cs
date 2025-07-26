using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.ValueObjects.Role;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace DotNetCleanTemplate.IntegrationTests.Infrastructure.Persistent.Repositories;

public class TestBaseRepository : BaseRepository
{
    public TestBaseRepository(AppDbContext context)
        : base(context) { }
}

public class BaseRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private AppDbContext _context = null!;
    private TestBaseRepository _repository = null!;

    public BaseRepositoryIntegrationTests()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        var connectionString = _postgresContainer.GetConnectionString();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        _context = new AppDbContext(options);
        await _context.Database.EnsureCreatedAsync();

        _repository = new TestBaseRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnEntity()
    {
        // Arrange
        var role = new Role(new RoleName("testrole"));
        await _repository.AddAsync(role);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync<Role>(role.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(role.Id, result.Id);
        Assert.Equal(role.Name.Value, result.Name.Value);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync<Role>(invalidId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        // Arrange
        var roles = new List<Role> { new(new RoleName("role1")), new(new RoleName("role2")) };

        foreach (var role in roles)
        {
            await _repository.AddAsync(role);
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync<Role>();

        // Assert
        Assert.True(result.Count() >= roles.Count);
        foreach (var role in roles)
        {
            Assert.Contains(result, r => r.Name.Value == role.Name.Value);
        }
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntity()
    {
        // Arrange
        var role = new Role(new RoleName("newrole"));

        // Act
        var result = await _repository.AddAsync(role);
        await _context.SaveChangesAsync();

        // Assert
        Assert.Equal(role, result);
        var savedRole = await _context.Set<Role>().FindAsync(role.Id);
        Assert.NotNull(savedRole);
        Assert.Equal(role.Name.Value, savedRole.Name.Value);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEntity()
    {
        // Arrange
        var role = new Role(new RoleName("originalrole"));
        await _repository.AddAsync(role);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.UpdateAsync(role);
        await _context.SaveChangesAsync();

        // Assert
        Assert.Equal(role, result);
        var updatedRole = await _context.Set<Role>().FindAsync(role.Id);
        Assert.NotNull(updatedRole);
        Assert.Equal(role.Name.Value, updatedRole.Name.Value);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteEntity()
    {
        // Arrange
        var role = new Role(new RoleName("todelete"));
        await _repository.AddAsync(role);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(role);
        await _context.SaveChangesAsync();

        // Assert
        Assert.Equal(role, result);
        var deletedRole = await _context.Set<Role>().FindAsync(role.Id);
        Assert.Null(deletedRole);
    }

    [Fact]
    public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
    {
        // Arrange
        var role = new Role(new RoleName("existingrole"));
        await _repository.AddAsync(role);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync<Role>(x => x.Id == role.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _repository.ExistsAsync<Role>(x => x.Id == invalidId);

        // Assert
        Assert.False(result);
    }
}
