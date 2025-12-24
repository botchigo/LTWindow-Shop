using MyShop.Domain.Entities;
using MyShop.Shared.DTOs.Dashboards;
using MyShop.Shared.DTOs.Products;

namespace MyShop.Application.Interfaces
{
    public interface IProductService
    {
        Task<Product> AddProductAsync(AddProductDTO request);
        Task<Product> UpdateProductAsync(UpdateProductDTO request);
        Task<Product> DeleteProductAsync(int productId);
        Task<DashboardData> GetDashboardDataAsync();
    }
}
