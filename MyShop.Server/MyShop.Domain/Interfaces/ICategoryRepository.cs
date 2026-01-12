using MyShop.Domain.Entities;

namespace MyShop.Domain.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<List<Category>> GetCategoriesAsync();
        Task<Category?> GetByNameAsync(string name);           
        Task<bool> IsNameExistAsync(string name);
        Task<Dictionary<string, int>> GetDictionaryAsync();
    }
}
