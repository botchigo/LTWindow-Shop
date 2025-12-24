using Microsoft.Extensions.Logging;
using MyShop.Domain.Entities;
using MyShop.Domain.Interfaces;

namespace MyShop.Infrastructure.Data.Repositories
{
    public class ProductImageRepository : GenericRepository<ProductImage>, IProductImageRepository
    {
        public ProductImageRepository(AppDbContext context, ILogger<ProductImageRepository> logger) : base(context, logger)
        {
        }
    }
}
