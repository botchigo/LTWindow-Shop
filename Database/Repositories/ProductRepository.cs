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
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetTotalProductAmountAsync(int categoryId, string? keyword, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Products.AsQueryable();

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
            var products = _context.Products.AsQueryable();

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
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product is null)
                return;

            try
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Product Repository] {ex.Message}");
            }
        }

        public async Task AddProductAsync(Product product)
        {
            try
            {
                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();
            }
            catch(Exception ex )
            {
                System.Diagnostics.Debug.WriteLine($"[Product Repository] {ex.Message}");
            }
        }

        public async Task UpdateProductAsync(Product product)
        {
            var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
            if (existingProduct is null)
                return;

            try
            {
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Product Repository] {ex.Message}");
            }
        }
    }
}
