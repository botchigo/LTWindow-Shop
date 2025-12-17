using Database.models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyShop.Services
{
    public interface IImportService
    {
        Task<List<Product>> ImportFromExcelAsync(string filePath);
        Task<List<Product>> ImportFromAccessAsync(string filePath);
    }
}
