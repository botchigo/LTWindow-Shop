using Database.models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetCategoriesAsync();
        Task<Category?> GetByNameAsync(string name);
        Task AddAsync(Category category);
        Task DeleteAsync(int id);
        Task<bool> IsCategoryUsedAsync(int id);
        Task<bool> IsNameExistAsync(string name);
    }
}
