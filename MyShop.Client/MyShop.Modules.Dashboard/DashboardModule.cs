using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Markup;
using MyShop.Contract;
using MyShop.Modules.Dashboard.ViewModels;
using MyShop.Modules.Dashboard.Views;
using System;
using System.Linq;
using System.Reflection;

namespace MyShop.Modules.Dashboard
{
    public class DashboardModule : IModule
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
            navService.Configure("DashboardPage", typeof(DashboardPage));
        }

        public void RegisterTypes(IServiceCollection services)
        {
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<DashboardPage>();
        }
    }
}
