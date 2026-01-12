using MyShop.Domain.Entities;
using MyShop.Domain.Interfaces;

namespace MyShop.API.GraphQL.Queries
{
    [ExtendObjectType("Query")]
    public class CategoryQueries
    {
        [UseOffsetPaging(IncludeTotalCount = true)]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Category> GetCategories([Service] IUnitOfWork unitOfWork)
        {
            return unitOfWork.Categories.GetQueryable();
        }
    }
}
