using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Factories.Entities;
using DotNetCleanTemplate.Domain.Factories.Role;
using DotNetCleanTemplate.Domain.Services;
using DotNetCleanTemplate.Infrastructure.Configurations;
using DotNetCleanTemplate.Infrastructure.Persistent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetCleanTemplate.Infrastructure.Services
{
    public class InitDataService : IInitDataService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<InitDataService> _logger;
        private readonly InitDataConfig _initDataConfig;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IRoleFactory _roleFactory;
        private readonly IUserFactory _userFactory;

        public InitDataService(
            AppDbContext dbContext,
            ILogger<InitDataService> logger,
            IOptions<InitDataConfig> initDataOptions,
            IPasswordHasher passwordHasher,
            IRoleFactory roleFactory,
            IUserFactory userFactory
        )
        {
            _dbContext = dbContext;
            _logger = logger;
            _initDataConfig = initDataOptions.Value;
            _passwordHasher = passwordHasher;
            _roleFactory = roleFactory;
            _userFactory = userFactory;
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
                var role = _roleFactory.Create(roleConfig.Name);
                _dbContext.Roles.Add(role);
            }
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Добавление пользователей
            foreach (var userConfig in config.Users)
            {
                if (_dbContext.Users.Any(u => u.Email.Value == userConfig.Email))
                    continue;

                var hashString = _passwordHasher.HashPassword(userConfig.Password);
                var user = _userFactory.Create(userConfig.UserName, userConfig.Email, hashString);
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
