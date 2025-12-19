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
            int categoryId, string? keyword = null,
            decimal? minPrice = null, decimal? maxPrice = null,
            SortCriteria selectedSortCriteria = SortCriteria.Default,
            SortDirection selectedSortDirection = SortDirection.Ascending);
        Task DeleteProductAsync(int productId);
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task<Product?> GetProductDetailsAsync(int productId);
        Task<bool> IsDuplicatedSku(string sku);
        Task AddRangeAsync(List<Product> products);
        Task<List<string>> GetAllSkuAsync();
    }
}
