using Database.models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetCategoriesAsync();
    }
}
