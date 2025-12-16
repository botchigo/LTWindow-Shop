using Database.Enums;
using Database.models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IProductRepository
    {
        Task<int> GetTotalProductAmountAsync(int categoryId, string? keyword, decimal? minPrice, decimal? maxPrice);
        Task<List<Product>> GetPagedProductsAsync(
            int pageNumber, int pageSize,
            int categoryId, string? keyword, decimal? minPrice, decimal? maxPrice,
            SortCriteria selectedSortCriteria, SortDirection selectedSortDirection);
        Task DeleteProductAsync(int productId);
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);
    }
}
