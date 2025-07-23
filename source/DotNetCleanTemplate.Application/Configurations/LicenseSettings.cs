using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCleanTemplate.Application.Configurations
{
    public class LicenseSettings
    {
        public const string SectionName = "License";
        public string? MediatrLicenseKey { get; set; }
    }
}
