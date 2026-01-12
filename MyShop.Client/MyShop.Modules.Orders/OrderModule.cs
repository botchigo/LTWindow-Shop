using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Markup;
using MyShop.Contract;
using MyShop.Modules.Orders.ViewModels;
using MyShop.Modules.Orders.Views;
using System;
using System.Linq;
using System.Reflection;

namespace MyShop.Modules.Orders
{
    public class OrderModule : IModule
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
            navService.Configure("OrderManagementPage", typeof(OrderManagementPage));
            navService.Configure("OrderDetailsPage", typeof(OrderDetailsPage));
            navService.Configure("CreateOrderPage", typeof(CreateOrderPage));
        }

        public void RegisterTypes(IServiceCollection services)
        {
            services.AddTransient<OrderManagementViewModel>();
            services.AddTransient<OrderDetailsViewModel>();
            services.AddTransient<CreateOrderViewModel>();

            services.AddTransient<OrderManagementPage>();
            services.AddTransient<OrderDetailsPage>();
            services.AddTransient<CreateOrderPage>();
        }
    }
}
