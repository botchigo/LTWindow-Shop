using Database.models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        public CategoryRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Categories.ToListAsync();
        }

        public async Task<Category?> GetByNameAsync(string name)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Categories.FirstOrDefaultAsync(c => c.Name == name);
        }

        public async Task AddAsync(Category category)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            await context.Categories.AddAsync(category);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var existing = await context.Categories.FindAsync(id);
            if(existing is not null)
            {
                context.Categories.Remove(existing);
                await context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsCategoryUsedAsync(int id)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Products.AnyAsync(p => p.CategoryId == id);
        }

        public async Task<bool> IsNameExistAsync(string name)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Categories.AnyAsync(c => c.Name.ToLower() == name.ToLower());
        }
    }
}
