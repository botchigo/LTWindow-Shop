using Database;
using Database.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using MyShop.Services;
using MyShop.ViewModels;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public IHost AppHost { get; private set; }
        public static Window? MainWindow { get; private set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    var connectionString = context
                        .Configuration
                        .GetConnectionString("DefaultConnection");

                    services.AddDbContextFactory<AppDbContext>(options =>
                    {
                        options.UseNpgsql(connectionString);
                    });

                    services.AddTransient<MainWindow>();

                    services.AddTransient<ICategoryRepository, CategoryRepository>();
                    services.AddTransient<DatabaseManager>();
                    services.AddTransient<IProductRepository, ProductRepository>();
                    services.AddTransient<IDialogService, DialogService>();
                    services.AddTransient<IImportService, ImportService>();
                    services.AddTransient<IFilePickerService, FilePickerService>();

                    services.AddTransient<ProductManagementViewModel>();
                    services.AddTransient<CategoryManagementViewModel>();
                })
                .Build();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            try
            {
                using var context = await AppHost.Services
                    .GetRequiredService<IDbContextFactory<AppDbContext>>()
                    .CreateDbContextAsync();

                await context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DB Initialization Error: {ex.Message}");
            }

            MainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            MainWindow.Activate();
        }

        public static T GetService<T>() where T : class
        {
            if ((Current as App)!.AppHost.Services.GetRequiredService(typeof(T)) is not T service)
            {
                throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
            }
            return service;
        }
    }
}
