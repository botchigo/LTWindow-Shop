using MyShop.Application.Commons;
using MyShop.Application.Interfaces;
using MyShop.Domain.Entities;
using MyShop.Shared.DTOs.Products;

namespace MyShop.API.GraphQL.Mutations
{
    [ExtendObjectType("Mutation")]
    public class ProductMutations
    {
        public async Task<TPayload<Product>> AddProductAsync(
            [Service] IProductService productService,
            AddProductDTO input)
        {
            try
            {
                var product = await productService.AddProductAsync(input);
                return new TPayload<Product>(product);
            }
            catch (Exception ex)
            {
                return new TPayload<Product>(ex.Message);
            }
        }

        public async Task<TPayload<List<Product>>> ImportProductsAsync(
            [Service] IProductService productService,
            List<ImportProductDTO> input)
        {
            try
            {
                var products = await productService.ImportProductsAsync(input);
                return new TPayload<List<Product>>(products);
            }
            catch (Exception ex)
            {
                return new TPayload<List<Product>>(ex.Message);
            }
        }

        public async Task<TPayload<Product>> UpdateProductAsync(
            [Service] IProductService productService,
            UpdateProductDTO input)
        {
            try
            {
                var product = await productService.UpdateProductAsync(input);
                return new TPayload<Product>(product);
            }
            catch (Exception ex)
            {
                return new TPayload<Product>(ex.Message);
            }
        }

        public async Task<TPayload<Product>> DeleteProductAsync(
            [Service] IProductService productService,
            int id)
        {
            try
            {
                var product = await productService.DeleteProductAsync(id);
                return new TPayload<Product>(product);
            }
            catch (Exception ex)
            {
                return new TPayload<Product>(ex.Message);
            }
        }
    }
}
