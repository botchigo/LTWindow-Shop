using AutoMapper;
using MyShop.Domain.Entities;
using MyShop.Shared.DTOs.Dashboards;
using MyShop.Shared.DTOs.Products;

namespace MyShop.Application.Mappers
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            CreateMap<AddProductDTO, Product>()
                .ForMember(dest =>
                    dest.ProductImages, opt =>
                    opt.MapFrom(src =>
                        src.ImagePaths.Select(path => new ProductImage
                        {
                            Path = path,
                        })));

            CreateMap<UpdateProductDTO, Product>();

            CreateMap<Product, LowStockProduct>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Id));

            CreateMap<Product, BestSellerProduct>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Id));
        }
    }
}
