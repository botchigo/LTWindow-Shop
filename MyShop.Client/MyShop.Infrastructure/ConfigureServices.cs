using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyShop.Core.Helpers;
using MyShop.Core.Interfaces;
using MyShop.Infrastructure.Authentication;
using Windows.Storage;

namespace MyShop.Infrastructure
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddClientInfrastructure(this IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string finalUrl = configuration["ApiSettings:BaseUrl"]!;

            string? customUrl = ConfigHelper.GetServerUrl();
            if (!string.IsNullOrEmpty(customUrl))
            {
                finalUrl = customUrl;
            }

            services.AddSingleton<IAuthenticationService, AuthenticationService>();
            services.AddTransient<AuthHeaderHandler>();

            services
                .AddMyShopClient()
                .ConfigureHttpClient(
                    client => client.BaseAddress = new Uri(finalUrl),
                    builder => builder.AddHttpMessageHandler<AuthHeaderHandler>()
                );

            return services;
        }
    }
}
