using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Markup;
using MyShop.Contract;
using MyShop.Modules.Report.ViewModels;
using MyShop.Modules.Report.Views;
using System;
using System.Linq;
using System.Reflection;

namespace MyShop.Modules.Report
{
    public class ReportModule : IModule
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
            navService.Configure("ReportPage", typeof(ReportPage));
        }

        public void RegisterTypes(IServiceCollection services)
        {
            services.AddTransient<ReportViewModel>();

            services.AddTransient<ReportPage>();
        }
    }
}
