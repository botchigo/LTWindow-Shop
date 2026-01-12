using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyShop.Domain.Entities;
using MyShop.Domain.Interfaces;
using MyShop.Shared.Helpers;

namespace MyShop.Infrastructure.Data.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext context, ILogger<CategoryRepository> logger) 
            : base(context, logger)
        {
        }

        public async Task<Dictionary<string, int>> GetDictionaryAsync()
        {
            var categories = await _dbSet
                .AsNoTracking()
                .Select(c => new { c.Name, c.Id })
                .ToListAsync();

            var dict = new Dictionary<string, int>();

            foreach (var cat in categories)
            {
                if (string.IsNullOrWhiteSpace(cat.Name)) continue;

                var key = StringHelper.GenerateSku(cat.Name.Trim());

                if (!dict.ContainsKey(key))
                {
                    dict.Add(key, cat.Id);
                }
            }

            return dict;
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<Category?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Name == name);
        }   

        public async Task<bool> IsNameExistAsync(string name)
        {
            return await _dbSet.AnyAsync(c => c.Name.ToLower() == name.ToLower());
        }
    }
}
