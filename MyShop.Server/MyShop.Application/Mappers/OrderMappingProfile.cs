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
            CreateMap<AddOrderDTO, Order>();
            CreateMap<OrderItemInfoDTO, OrderItem>();

            CreateMap<Order, RecentOrder>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CreatedTime, opt => opt.MapFrom(src => src.CreatedAt));
        }
    }
}
