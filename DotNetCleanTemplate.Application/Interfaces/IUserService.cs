using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;

namespace DotNetCleanTemplate.Application.Interfaces
{
    public interface IUserService
    {
        Task<Result<User>> FindByEmailAsync(
            string email,
            CancellationToken cancellationToken = default
        );
        Task<Result<User>> CreateUserAsync(
            User user,
            CancellationToken cancellationToken = default
        );
        Task<Result<List<User>>> GetAllUsersWithRolesAsync(
            CancellationToken cancellationToken = default
        );
    }
}
