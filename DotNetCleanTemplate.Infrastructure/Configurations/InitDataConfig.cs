using System.Collections.Generic;

namespace DotNetCleanTemplate.Infrastructure.Configurations
{
    public class InitDataConfig
    {
        public List<InitRoleConfig> Roles { get; set; } = new();
        public List<InitUserConfig> Users { get; set; } = new();
    }

    public class InitRoleConfig
    {
        public string Name { get; set; } = null!;
    }

    public class InitUserConfig
    {
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public List<string> Roles { get; set; } = new();
    }
}
