using AutoMapper;
using EShopApi.Models.DTO;
using EShopApi.Models;

namespace EShopApi.Profiles
{
    public class AuthProfile : Profile
    {
        public AuthProfile() 
        {
            CreateMap<User, UserDto>();

        }
    }
}
