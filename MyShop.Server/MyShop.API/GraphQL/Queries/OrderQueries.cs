using MyShop.Domain.Entities;
using MyShop.Domain.Interfaces;

namespace MyShop.API.GraphQL.Queries
{
    [ExtendObjectType("Query")]
    public class OrderQueries
    {
        [UseOffsetPaging(IncludeTotalCount = true)]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Order> GetOrders([Service] IUnitOfWork unitOfWork)
        {
            return unitOfWork.Orders.GetQueryable();
        }

        [UseSingleOrDefault]
        [UseProjection]
        public IQueryable<Order> GetOrderById(
            [Service] IUnitOfWork unitOfWork,
            int orderId)
        {
            return unitOfWork.Orders.GetQueryable().Where(o => o.Id == orderId);
        }
    }
}
