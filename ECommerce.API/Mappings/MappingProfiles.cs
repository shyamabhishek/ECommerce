using AutoMapper;
using ECommerce.API.Models;
using ECommerce.API.DTOs;

namespace ECommerce.API.Mappings
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles() { CreateMap<Product, ProductDto>().ReverseMap(); }
    }
}
