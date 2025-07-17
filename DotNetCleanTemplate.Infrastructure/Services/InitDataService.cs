using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Services;
using DotNetCleanTemplate.Domain.ValueObjects.Role;
using DotNetCleanTemplate.Domain.ValueObjects.User;
using DotNetCleanTemplate.Infrastructure.Configurations;
using DotNetCleanTemplate.Infrastructure.Persistent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetCleanTemplate.Infrastructure.Services
{
    public class InitDataService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<InitDataService> _logger;
        private readonly InitDataConfig _initDataConfig;
        private readonly IPasswordHasher _passwordHasher;

        public InitDataService(
            AppDbContext dbContext,
            ILogger<InitDataService> logger,
            IOptions<InitDataConfig> initDataOptions,
            IPasswordHasher passwordHasher
        )
        {
            _dbContext = dbContext;
            _logger = logger;
            _initDataConfig = initDataOptions.Value;
            _passwordHasher = passwordHasher;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            var config = _initDataConfig;
            if (config == null || (config.Roles.Count == 0 && config.Users.Count == 0))
            {
                _logger.LogWarning("Init data config is empty or invalid.");
                return;
            }
            _logger.LogInformation("Initializing data...");
            // Добавление ролей
            foreach (
                var roleConfig in config.Roles.Where(rc =>
                    !_dbContext.Roles.Any(r => r.Name.Value == rc.Name)
                )
            )
            {
                var role = new Role(new RoleName(roleConfig.Name));
                _dbContext.Roles.Add(role);
            }
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Добавление пользователей
            foreach (var userConfig in config.Users)
            {
                if (_dbContext.Users.Any(u => u.Email.Value == userConfig.Email))
                    continue;

                var hashString = _passwordHasher.HashPassword(userConfig.Password);
                var user = new User(
                    new UserName(userConfig.UserName),
                    new Email(userConfig.Email),
                    new PasswordHash(hashString)
                );
                // Привязываем роли
                foreach (var roleName in userConfig.Roles)
                {
                    var role = _dbContext.Roles.FirstOrDefault(r => r.Name.Value == roleName);
                    if (role != null)
                    {
                        user.AssignRole(role);
                    }
                }
                _dbContext.Users.Add(user);
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Data initialized successfully.");
        }
    }
}
