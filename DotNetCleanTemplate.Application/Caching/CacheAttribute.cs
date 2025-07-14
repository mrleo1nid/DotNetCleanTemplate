using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCleanTemplate.Application.Caching
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class CacheAttribute : Attribute
    {
        public string Key { get; }
        public string? Region { get; set; }

        public CacheAttribute(string key, string? region = null)
        {
            Key = key;
            Region = region;
        }
    }
}
