using AutoMapper;
using MyShop.Domain.Entities;
using MyShop.Shared.DTOs.Users;

namespace MyShop.Application.Mappers
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<RegisterDTO, AppUser>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        }
    }
}
