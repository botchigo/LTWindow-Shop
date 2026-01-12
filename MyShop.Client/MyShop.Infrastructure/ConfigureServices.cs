using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyShop.Core.Interfaces;
using MyShop.Infrastructure.Authentication;

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

            string apiUrl = configuration["ApiSettings:BaseUrl"]!;

            services.AddSingleton<IAuthenticationService, AuthenticationService>();
            services.AddTransient<AuthHeaderHandler>();

            services
                .AddMyShopClient()
                .ConfigureHttpClient(
                    client => client.BaseAddress = new Uri(apiUrl),
                    builder => builder.AddHttpMessageHandler<AuthHeaderHandler>()
                );

            return services;
        }
    }
}
