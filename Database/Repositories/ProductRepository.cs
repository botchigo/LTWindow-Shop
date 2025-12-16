using Database.Enums;
using Database.models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        public ProductRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<int> GetTotalProductAmountAsync(int categoryId, string? keyword, decimal? minPrice, decimal? maxPrice)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            var query = context.Products.AsQueryable();

            if (categoryId != 0) query = query.Where(p => p.CategoryId == categoryId);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(p => p.Name.Contains(keyword) || p.Description.Contains(keyword));

            if (minPrice.HasValue)
                query = query.Where(p => p.ImportPrice >= minPrice);

            if (maxPrice.HasValue)
                query = query.Where(p => p.ImportPrice <= maxPrice);

            return await query.CountAsync();
        }

        public async Task<List<Product>> GetPagedProductsAsync(
            int pageNumber, int pageSize,
            int categoryId, string? keyword, decimal? minPrice, decimal? maxPrice,
            SortCriteria selectedSortCriteria, SortDirection selectedSortDirection)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            var products = context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (categoryId != 0)
                products = products.Where(p => p.CategoryId == categoryId);

            if (!string.IsNullOrEmpty(keyword))
                products = products.Where(p => p.Name.Contains(keyword) || p.Description.Contains(keyword));

            if (minPrice.HasValue)
                products = products.Where(p => p.ImportPrice >= minPrice);

            if (maxPrice.HasValue)
                products = products.Where(p => p.ImportPrice <= maxPrice);

            IOrderedQueryable<Product> orderedProducts = selectedSortDirection == SortDirection.Ascending
                ? (selectedSortCriteria switch
                {
                    SortCriteria.Name => products.OrderBy(p => p.Name),
                    SortCriteria.Price => products.OrderBy(p => p.ImportPrice),
                    SortCriteria.CreateDate => products.OrderBy(p => p.CreatedAt),
                    SortCriteria.UpdateDate => products.OrderBy(p => p.UpdatedAt),
                    _ => products.OrderBy(p => p.Id)
                })
                : (selectedSortCriteria switch
                {
                    SortCriteria.Name => products.OrderByDescending(p => p.Name),
                    SortCriteria.Price => products.OrderByDescending(p => p.ImportPrice),
                    SortCriteria.CreateDate => products.OrderByDescending(p => p.CreatedAt),
                    SortCriteria.UpdateDate => products.OrderByDescending(p => p.UpdatedAt),
                    _ => products.OrderBy(p => p.Id)
                });

            if (selectedSortCriteria != SortCriteria.Default)
                orderedProducts = (selectedSortDirection == SortDirection.Ascending)
                    ? orderedProducts.ThenBy(p => p.Id)
                    : orderedProducts.ThenByDescending(p => p.Id);

            var pagedProducts = await orderedProducts
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return pagedProducts;
        }

        public async Task DeleteProductAsync(int productId)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            var product = await context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product is null)
                return;

            try
            {
                context.Products.Remove(product);
                await context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Product Repository] {ex.Message}");
            }
        }

        public async Task AddProductAsync(Product product)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            try
            {
                await context.Products.AddAsync(product);
                await context.SaveChangesAsync();
            }
            catch(Exception ex )
            {
                System.Diagnostics.Debug.WriteLine($"[Product Repository] {ex.Message}");
            }
        }

        public async Task UpdateProductAsync(Product product)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            var existingProduct = await context.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
            if (existingProduct is null)
                return;

            try
            {
                existingProduct.Name = product.Name;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.ImportPrice = product.ImportPrice;
                existingProduct.SalePrice = product.SalePrice;
                existingProduct.Description = product.Description;
                existingProduct.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Product Repository] {ex.Message}");
            }
        }
    }
}
