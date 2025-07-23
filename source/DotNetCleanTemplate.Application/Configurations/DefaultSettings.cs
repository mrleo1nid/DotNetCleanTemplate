using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCleanTemplate.Application.Configurations
{
    public class DefaultSettings
    {
        public static readonly string SectionName = "DefaultSettings";
        public string? DefaultUserRole { get; set; } = "User";
        public string? DefaultAdminRole { get; set; } = "Admin";
    }
}
