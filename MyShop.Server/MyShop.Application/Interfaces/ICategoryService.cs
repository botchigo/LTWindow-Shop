using MyShop.Domain.Entities;
using MyShop.Shared.DTOs.Categories;

namespace MyShop.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<Category> AddCategoryAsync(AddCategoryDTO request);
        Task<Category> DeleteCategoryAsync(int categoryId);
    }
}
