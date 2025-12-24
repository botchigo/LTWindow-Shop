using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyShop.Domain.Entities;
using MyShop.Domain.Interfaces;

namespace MyShop.Infrastructure.Data.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext context, ILogger<CategoryRepository> logger) 
            : base(context, logger)
        {
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
