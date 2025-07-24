using DotNetCleanTemplate.Application.Configurations;
using DotNetCleanTemplate.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace DotNetCleanTemplate.Application.Services
{
    public class DefaultRoleService : IDefaultRoleService
    {
        private readonly DefaultSettings _defaultSettings;

        public DefaultRoleService(IOptions<DefaultSettings> defaultSettings)
        {
            _defaultSettings = defaultSettings.Value;
        }

        public bool IsDefaultRole(string roleName)
        {
            return roleName.Equals(
                    _defaultSettings.DefaultAdminRole,
                    StringComparison.OrdinalIgnoreCase
                )
                || roleName.Equals(
                    _defaultSettings.DefaultUserRole,
                    StringComparison.OrdinalIgnoreCase
                );
        }
    }
}
