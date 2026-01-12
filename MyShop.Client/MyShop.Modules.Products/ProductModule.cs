using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Markup;
using MyShop.Contract;
using MyShop.Modules.Products.ViewModels;
using MyShop.Modules.Products.Views;
using System;
using System.Linq;
using System.Reflection;

namespace MyShop.Modules.Products
{
    public class ProductModule : IModule
    {
        public IXamlMetadataProvider GetMetadataProvider()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var providerType = assembly.GetTypes()
                .FirstOrDefault(t => t.Name == "XamlMetaDataProvider"
                                  && typeof(IXamlMetadataProvider).IsAssignableFrom(t));

            if (providerType != null)
            {
                return (IXamlMetadataProvider)Activator.CreateInstance(providerType);
            }
            return null;
        }

        public void MapNavigation(INavigationService navService)
        {
            navService.Configure("ProductListPage", typeof(ProductListPage));
            navService.Configure("ProductDetailsPage", typeof(ProductDetailsPage));
            navService.Configure("ProductEditorPage", typeof(ProductEditorPage));
            navService.Configure("CategoryManagementPage", typeof(CategoryManagementPage));
        }

        public void RegisterTypes(IServiceCollection services)
        {
            services.AddTransient<ProductListViewModel>();
            services.AddTransient<ProductDetailsViewModel>();
            services.AddTransient<ProductEditorViewModel>();
            services.AddTransient<CategoryManagementViewModel>();

            services.AddTransient<ProductListPage>();
            services.AddTransient<ProductDetailsPage>();
            services.AddTransient<ProductEditorPage>();
            services.AddTransient<CategoryManagementPage>();
        }
    }
}
