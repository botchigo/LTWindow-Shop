using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using MyShop.Models;
using MyShop.Services;
using MyShop.ViewModels;

namespace MyShop
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private static IHost? _host;
        internal Window? m_window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            
            // Setup Dependency Injection
            _host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Add appsettings.json
                    var appPath = AppContext.BaseDirectory;
                    config.SetBasePath(appPath);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    // Configuration
                    services.Configure<DatabaseSettings>(context.Configuration.GetSection("Database"));

                    // Services
                    services.AddSingleton<DatabaseManager>();
                    services.AddTransient<DatabaseDebugger>();

                    // ViewModels
                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<SignupViewModel>();
                    services.AddTransient<DashboardViewModel>();

                    // MainWindow
                    services.AddSingleton<MainWindow>();
                })
                .Build();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = GetService<MainWindow>();
            m_window.Activate();
        }

        public static T GetService<T>() where T : class
        {
            if (_host == null)
                throw new InvalidOperationException("Host is not initialized");

            return _host.Services.GetRequiredService<T>();
        }
    }
}
