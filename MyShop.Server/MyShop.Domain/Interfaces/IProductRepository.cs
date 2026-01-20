using MyShop.Domain.Entities;
using MyShop.Shared.DTOs.Dashboards;
using MyShop.Shared.DTOs.Reports;
using MyShop.Shared.Enums;

namespace MyShop.Domain.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        IQueryable<Product> GetProductQueryable(string? keyword, int? categoryId);
        Task<Product?> GetWithImagesAsync(int id);   
        Task<Product?> GetProductDetailsAsync(int productId);
        Task<bool> IsDuplicatedSku(string sku);
        Task<List<string>> GetAllSkuAsync();
        Task<List<Product>> GetByIdsAsync(List<int> ids);
        Task<bool> IsCategoryUsedAsync(int id);

        //report
        Task<List<ReportDataPoint>> GetProductSalesReportAsync(DateTime start, DateTime end, ReportTimeInterval interval);
        Task<List<ReportDataPoint>> GetRevenueProfitReportAsync(DateTime start, DateTime end, ReportTimeInterval interval);
        Task<List<ProductSeries>> GetProductComparisonReportAsync(DateTime start, DateTime end, ReportTimeInterval interval, List<string> productNames);
        Task<ProductSeries> GetSingleProductSeriesAsync(string productName, DateTime start, DateTime end, 
            ReportTimeInterval interval, int colorIndex);

        //dashboard
        Task<int> GetTotalProductsAsync();
        Task<List<LowStockProduct>> GetTop5LowStockAsync();
        Task<List<BestSellerProduct>> GetTop5BestSellersAsync();      
    }
}
