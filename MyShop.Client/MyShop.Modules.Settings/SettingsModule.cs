using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Markup;
using MyShop.Contract;
using MyShop.Modules.Settings.ViewModels;
using MyShop.Modules.Settings.Views;

namespace MyShop.Modules.Settings
{
    public class SettingsModule : IModule
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
            navService.Configure("SettingsPage", typeof(SettingsPage));
        }

        public void RegisterTypes(IServiceCollection services)
        {
            services.AddTransient<SettingsViewModel>();

            services.AddTransient<SettingsPage>();
        }
        public void RegisterNavigation(INavigationService navigationService)
        {
            // 2. [QUAN TRỌNG] Map Key "SettingsPage" vào Class SettingsPage
            navigationService.Configure("SettingsPage", typeof(SettingsPage));
        }
    }
}
