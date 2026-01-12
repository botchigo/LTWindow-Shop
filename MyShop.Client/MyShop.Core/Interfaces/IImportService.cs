using MyShop.Core.DTOs;

namespace MyShop.Core.Interfaces
{
    public interface IImportService
    {
        Task<List<ProductImportDto>> ParseExcelAsync(string filePath);
        Task<List<ProductImportDto>> ParseAccessAsync(string filePath);
    }
}
