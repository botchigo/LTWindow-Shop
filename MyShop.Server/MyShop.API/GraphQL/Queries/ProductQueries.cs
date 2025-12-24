using MyShop.Application.Interfaces;
using MyShop.Domain.Entities;
using MyShop.Domain.Interfaces;
using MyShop.Shared.DTOs.Dashboards;
using MyShop.Shared.DTOs.Reports;

namespace MyShop.API.GraphQL.Queries
{
    [ExtendObjectType("Query")]
    public class ProductQueries
    {
        [UseOffsetPaging(IncludeTotalCount = true)]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Product> GetProducts([Service] IUnitOfWork unitOfWork)
        {
            return unitOfWork.Products.GetQueryable();
        }

        [UseFirstOrDefault]
        [UseProjection]
        public IQueryable<Product> GetProductById(
            [Service] IUnitOfWork unitOfWork,
            int id)
        {
            return unitOfWork.Products.GetQueryable().Where(p => p.Id == id);
        }

        //report
        public async Task<List<ReportDataPoint>> GetProductSalesReportAsync(
            [Service] IUnitOfWork unitOfWork,
            ReportBaseParams request)
        {
            return await unitOfWork.Products.GetProductSalesReportAsync(request.start, request.end, request.interval);
        }

        public async Task<List<ProductSeries>> GetProductComparisonReportAsync(
            [Service] IUnitOfWork unitOfWork,
            ReportBaseParams request)
        {
            return await unitOfWork.Products.GetProductComparisonReportAsync(request.start, request.end, request.interval);
        }

        public async Task<List<ReportDataPoint>> GetRevenueProfitReportAsync(
            [Service] IUnitOfWork unitOfWork,
            ReportBaseParams request)
        {
            return await unitOfWork.Products.GetRevenueProfitReportAsync(request.start, request.end, request.interval);
        }

        public async Task<ProductSeries> GetSingleProductSeriesAsync(
            [Service] IUnitOfWork unitOfWork,
            GetSingleProductSeriesDTO request)
        {
            return await unitOfWork.Products.GetSingleProductSeriesAsync(
                request.productName,
                request.start, 
                request.end, 
                request.interval, 
                request.colorIndex);
        }

        //dashboard
        public async Task<int> GetTotalProductsAsync([Service] IUnitOfWork unitOfWork)
        {
            return await unitOfWork.Products.GetTotalProductsAsync();
        }

        public async Task<List<LowStockProduct>> GetTop5LowStockAsync([Service] IUnitOfWork unitOfWork)
        {
            return await unitOfWork.Products.GetTop5LowStockAsync();
        }

        public async Task<List<BestSellerProduct>> GetTop5BestSellersAsync([Service] IUnitOfWork unitOfWork)
        {
            return await unitOfWork.Products.GetTop5BestSellersAsync();
        }

        public async Task<int> GetTotalOrdersTodayAsync([Service] IUnitOfWork unitOfWork)
        {
            return await unitOfWork.Orders.GetTotalOrdersTodayAsync();
        }

        public async Task<decimal> GetTotalRevenueTodayAsync([Service] IUnitOfWork unitOfWork)
        {
            return await unitOfWork.Orders.GetTotalRevenueTodayAsync();
        }

        public async Task<List<RecentOrder>> GetLatest3OrdersAsync([Service] IUnitOfWork unitOfWork)
        {
            return await unitOfWork.Orders.GetLatest3OrdersAsync();
        }

        public async Task<List<DailyRevenue>> GetRevenueByDayCurrentMonthAsync([Service] IUnitOfWork unitOfWork)
        {
            return await unitOfWork.Orders.GetRevenueByDayCurrentMonthAsync();
        }

        public async Task<DashboardData> GetDashboardDataAsync([Service] IProductService productService)
        {
            return await productService.GetDashboardDataAsync();
        }
    }
}
