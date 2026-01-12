using MyShop.Application.Commons;
using MyShop.Application.Interfaces;
using MyShop.Domain.Entities;
using MyShop.Shared.DTOs.Categories;

namespace MyShop.API.GraphQL.Mutations
{
    [ExtendObjectType("Mutation")]
    public class CategoryMutations
    {
        public async Task<TPayload<Category>> AddCategoryAsync(
            [Service] ICategoryService categoryService,
            AddCategoryDTO request)
        {
            try
            {
                var category = await categoryService.AddCategoryAsync(request);
                return new TPayload<Category>(category);
            }
            catch(Exception ex)
            {
                return new TPayload<Category>(ex.Message);
            }
        }

        public async Task<TPayload<Category>> DeleteCategoryAsync(
            [Service] ICategoryService categoryService,
            int categoryId)
        {
            try
            {
                var category = await categoryService.DeleteCategoryAsync(categoryId);
                return new TPayload<Category>(category);
            }
            catch (Exception ex)
            {
                return new TPayload<Category>(ex.Message);
            }
        }

    }
}
