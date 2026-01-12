using MyShop.Application.Commons;
using MyShop.Application.Interfaces;
using MyShop.Domain.Entities;
using MyShop.Shared.DTOs.Orders;

namespace MyShop.API.GraphQL.Mutations
{
    [ExtendObjectType("Mutation")]
    public class OrderMutations
    {
        public async Task<TPayload<Order>> CreateOrderAsync(
            [Service] IOrderService orderService,
            AddOrderDTO request)
        {
            try
            {
                var order = await orderService.CreateOrderAsync(request);
                return new TPayload<Order>(order);
            }
            catch(Exception ex)
            {
                return new TPayload<Order>(ex.Message);
            }
        }

        public async Task<TPayload<Order>> DeleteOrderAsync(
            [Service] IOrderService orderService,
            int orderId)
        {
            try
            {
                var order = await orderService.DeleteOrderAsync(orderId);
                return new TPayload<Order>(order);
            }
            catch(Exception ex)
            {
                return new TPayload<Order>(ex.Message);
            }
        }

        public async Task<TPayload<Order>> UpdateOrderStatusAsync(
            [Service] IOrderService orderService,
            UpdateOrderStatusDTO request)
        {
            try
            {
                var order = await orderService.UpdateStatusAsync(request);
                return new TPayload<Order>(order);
            }
            catch (Exception ex)
            {
                return new TPayload<Order>(ex.Message);
            }
        }
    }
}
