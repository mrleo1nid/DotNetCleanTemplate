using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.ValueObjects.User;
using DotNetCleanTemplate.Shared.DTOs;
using Mapster;

namespace DotNetCleanTemplate.Application.Mapping
{
#pragma warning disable S2325
    public class UserMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // User -> UserWithRolesDto
            config
                .NewConfig<User, UserWithRolesDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.UserName, src => src.Name.Value)
                .Map(dest => dest.Email, src => src.Email.Value)
                .Map(
                    dest => dest.Roles,
                    src => src.UserRoles.Select(ur => ur.Role).Adapt<List<RoleDto>>()
                );

            // Role -> RoleDto
            config
                .NewConfig<Role, RoleDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Name, src => src.Name.Value);
        }
    }
#pragma warning restore S2325
}
