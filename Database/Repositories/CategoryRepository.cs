using Database.models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
    }
}
