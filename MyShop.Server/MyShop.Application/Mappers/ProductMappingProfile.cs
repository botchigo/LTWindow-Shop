using AutoMapper;
using MyShop.Domain.Entities;
using MyShop.Shared.DTOs.Dashboards;
using MyShop.Shared.DTOs.Products;
using MyShop.Shared.Helpers;

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
                        })))
                .ForMember(dest =>
                    dest.Sku, opt =>
                    opt.MapFrom(src =>
                        StringHelper.GenerateSku(src.Name, null)));

            CreateMap<UpdateProductDTO, Product>();

            CreateMap<Product, LowStockProduct>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Id));

            CreateMap<Product, BestSellerProduct>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.TotalQuantity,  opt => opt.MapFrom(src => (long)src.SaleAmount));

            CreateMap<ImportProductDTO, Product>();
        }
    }
}
