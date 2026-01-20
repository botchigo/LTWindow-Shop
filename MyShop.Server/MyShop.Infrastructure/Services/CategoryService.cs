using AutoMapper;
using Microsoft.Extensions.Logging;
using MyShop.Application.Interfaces;
using MyShop.Domain.Entities;
using MyShop.Domain.Interfaces;
using MyShop.Shared.DTOs.Categories;

namespace MyShop.Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CategoryService> _logger;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork unitOfWork, ILogger<CategoryService> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Category> AddCategoryAsync(AddCategoryDTO request)
        {
            try
            {
                var category = _mapper.Map<Category>(request);
                if (await _unitOfWork.Categories.IsNameExistAsync(category.Name))
                    throw new Exception($"Loại {category.Name} đã tồn tại.");

                await _unitOfWork.Categories.AddAsync(category);
                await _unitOfWork.CompleteAsync();
                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<Category> DeleteCategoryAsync(int categoryId)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
            if (category is null)
                throw new Exception($"Loại không tồn tại");

            var isUsed = await _unitOfWork.Products.IsCategoryUsedAsync(categoryId);
            if (isUsed)
                throw new Exception($"Loại đang được sử dụng, không thể xóa.");

            try
            {
                _unitOfWork.Categories.Remove(category);
                await _unitOfWork.CompleteAsync();
                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
    }
}
