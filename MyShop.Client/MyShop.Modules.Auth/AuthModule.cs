using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Markup;
using MyShop.Contract;
using MyShop.Modules.Auth.ViewModels;
using MyShop.Modules.Auth.Views;
using System;
using System.Linq;
using System.Reflection;

namespace MyShop.Modules.Auth
{
    public class AuthModule : IModule
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
            navService.Configure("LoginPage", typeof(LoginPage));
            navService.Configure("RegisterPage", typeof(RegisterPage));
        }

        public void RegisterTypes(IServiceCollection services)
        {
            services.AddTransient<LoginViewModel>();
            services.AddTransient<RegisterViewModel>();

            services.AddTransient<LoginPage>();
            services.AddTransient<RegisterPage>();
        }
    }
}
