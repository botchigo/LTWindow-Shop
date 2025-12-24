using AutoMapper;
using MyShop.Domain.Entities;
using MyShop.Shared.DTOs.Categories;

namespace MyShop.Application.Mappers
{
    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            CreateMap<AddCategoryDTO, Category>();
        }
    }
}
