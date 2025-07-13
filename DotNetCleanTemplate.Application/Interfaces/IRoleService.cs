using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Shared.Common;

namespace DotNetCleanTemplate.Application.Interfaces
{
    public interface IRoleService
    {
        Task<Result<Role>> FindByNameAsync(
            string name,
            CancellationToken cancellationToken = default
        );
        Task<Result<Role>> CreateRoleAsync(
            Role role,
            CancellationToken cancellationToken = default
        );
    }
}
