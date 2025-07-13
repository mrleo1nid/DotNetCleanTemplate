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
        }
    }
}
