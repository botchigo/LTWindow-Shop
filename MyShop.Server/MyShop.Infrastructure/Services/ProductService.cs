using AutoMapper;
using Microsoft.Extensions.Logging;
using MyShop.Application.Interfaces;
using MyShop.Domain.Entities;
using MyShop.Domain.Interfaces;
using MyShop.Shared.DTOs.Dashboards;
using MyShop.Shared.DTOs.Products;
using MyShop.Shared.Helpers;

namespace MyShop.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductService> _logger;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, ILogger<ProductService> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<DashboardData> GetDashboardDataAsync()
        {
            var taskTotalProducts = _unitOfWork.Products.GetTotalProductsAsync();
            var taskOrdersToday = _unitOfWork.Orders.GetTotalOrdersTodayAsync();
            var taskRevenueToday = _unitOfWork.Orders.GetTotalRevenueTodayAsync();
            var taskLowStock = _unitOfWork.Products.GetTop5LowStockAsync();
            var taskBestSellers = _unitOfWork.Products.GetTop5BestSellersAsync();
            var taskRecentOrders = _unitOfWork.Orders.GetLatest3OrdersAsync();
            var taskMonthlyRevenue = _unitOfWork.Orders.GetRevenueByDayCurrentMonthAsync();

            await Task.WhenAll(
                taskTotalProducts, 
                taskOrdersToday, 
                taskRevenueToday,
                taskLowStock, 
                taskBestSellers, 
                taskRecentOrders, 
                taskMonthlyRevenue);

            return new DashboardData
            {
                TotalProducts = taskTotalProducts.Result,
                TodayOrders = taskOrdersToday.Result,
                TodayRevenue = taskRevenueToday.Result,
                LowStockProducts = taskLowStock.Result,
                BestSellerProducts = taskBestSellers.Result,
                RecentOrders = taskRecentOrders.Result,
                MonthlyRevenue = taskMonthlyRevenue.Result
            };
        }

        public async Task<Product> DeleteProductAsync(int productId)
        {
            var product = await _unitOfWork.Products.GetWithImagesAsync(productId);
            if (product is null)
                throw new Exception("Không tìm thấy sản phẩm");

            try
            {
                _unitOfWork.Products.Remove(product);
                await _unitOfWork.CompleteAsync();
                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<Product> AddProductAsync(AddProductDTO request)
        {
            if(request.ImportPrice > request.SalePrice)
                throw new Exception("Giá bán không được thấp hơn giá nhập!");

            try
            {
                var product = _mapper.Map<Product>(request);

                await _unitOfWork.Products.AddAsync(product);
                await _unitOfWork.CompleteAsync();

                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi thêm sản phẩm: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Product>> ImportProductsAsync(IEnumerable<ImportProductDTO> request)
        {
            try
            {
                var products = new List<Product>();

                var categoryDic = await _unitOfWork.Categories.GetDictionaryAsync();

                try
                {
                    var jsonDebug = System.Text.Json.JsonSerializer.Serialize(categoryDic, new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true, 
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping 
                    });

                    _logger.LogInformation("--- CHECK DICTIONARY SERVER ---");
                    _logger.LogInformation(jsonDebug);
                    _logger.LogInformation("-------------------------------");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Không thể log dictionary: " + ex.Message);
                }

                foreach (var item in request)
                {
                    if (string.IsNullOrWhiteSpace(item.CategoryName)) continue;                    

                    var product = _mapper.Map<Product>(item);

                    var sku = StringHelper.GenerateSku(product.Name);
                    var baseSku = sku;
                    int count = 1;
                    while(await _unitOfWork.Products.IsDuplicatedSku(sku))
                    {
                        sku = $"{baseSku}-{count}";
                        count++;
                    }
                    product.Sku = sku;

                    var inputCategoryName = item.CategoryName.Trim();
                    var lookupKey = StringHelper.GenerateSku(inputCategoryName);

                    if (!categoryDic.TryGetValue(StringHelper.GenerateSku(item.CategoryName), out var categoryId))
                    {
                        _logger.LogWarning($"Không tìm thấy danh mục khớp với: {item.CategoryName} (Key: {lookupKey})");
                        continue;
                    }
                    product.CategoryId = categoryId;

                    products.Add(product);
                }

                if (products.Count == 0)
                    throw new Exception("Không có sản phẩm nào hợp lệ");

                await _unitOfWork.Products.AddRangeAsync(products);
                await _unitOfWork.CompleteAsync();

                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi thêm sản phẩm: {ex.Message}");
                throw;
            }
        }

        public async Task<Product> UpdateProductAsync(UpdateProductDTO request)
        {
            var existingProduct = await _unitOfWork.Products.GetWithImagesAsync(request.Id);
            if (existingProduct is null)
                throw new Exception("Sản phẩm không tồn tại");

            if (request.ImportPrice > request.SalePrice)
                throw new Exception("Giá bán không được thấp hơn giá nhập!");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _mapper.Map(request, existingProduct);

                //delete images
                if(request.ImageIdToDelete is not null && request.ImageIdToDelete.Any())
                {
                    var imageToDelete = existingProduct.ProductImages
                        .Where(i => request.ImageIdToDelete.Contains(i.Id))
                        .ToList();
                    if(imageToDelete is not null && imageToDelete.Any())
                        _unitOfWork.ProductImages.RemoveRange(imageToDelete);
                }

                //add images
                if(request.NewImagePaths is not null && request.NewImagePaths.Any())
                {
                    var existingImagePaths = existingProduct.ProductImages.Select(i => i.Path).ToList();
                    var newImage = request.NewImagePaths
                        .Where(path => !existingImagePaths.Contains(path))
                        .Select(path => new ProductImage
                        {
                            Path = path,
                            ProductId = existingProduct.Id,
                        })
                        .ToList();
                    if (newImage is not null && newImage.Any())
                        await _unitOfWork.ProductImages.AddRangeAsync(newImage);
                }

                await _unitOfWork.CommitTransactionAsync();
                return existingProduct;

            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex.Message);
                throw;
            }
        }
    }
}
