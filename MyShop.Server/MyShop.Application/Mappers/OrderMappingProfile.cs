using AutoMapper;
using MyShop.Domain.Entities;
using MyShop.Shared.DTOs.Dashboards;
using MyShop.Shared.DTOs.Orders;

namespace MyShop.Application.Mappers
{
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile()
        {
            CreateMap<AddOrderDTO, Order>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod));

            CreateMap<OrderItemInfoDTO, OrderItem>();

            CreateMap<Order, RecentOrder>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CreatedTime, opt => opt.MapFrom(src => src.CreatedAt));
        }
    }
}
