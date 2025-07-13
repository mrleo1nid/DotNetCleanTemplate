using AutoMapper;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.ValueObjects.User;
using DotNetCleanTemplate.Shared.DTOs;

namespace DotNetCleanTemplate.Application.Mapping
{
    public class RegisterUserMappingProfile : Profile
    {
        public RegisterUserMappingProfile()
        {
            CreateMap<RegisterUserDto, User>()
                .ConvertUsing(dto => new User(
                    new UserName(dto.UserName),
                    new Email(dto.Email),
                    new PasswordHash(dto.Password)
                ));
            CreateMap<
                DotNetCleanTemplate.Domain.Entities.User,
                DotNetCleanTemplate.Shared.DTOs.UserWithRolesDto
            >()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Name.Value))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
                .ForMember(
                    dest => dest.Roles,
                    opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role))
                );
            CreateMap<
                DotNetCleanTemplate.Domain.Entities.Role,
                DotNetCleanTemplate.Shared.DTOs.RoleDto
            >()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Value));
        }
    }
}
