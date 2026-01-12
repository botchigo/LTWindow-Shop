using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.XamlTypeInfo;
using MyShop.Contract;
using MyShop.Core.Helpers;
using MyShop.Core.Interfaces;
using MyShop.Infrastructure;
using MyShop.Infrastructure.Authentication;
using MyShop.Infrastructure.Services;
using MyShop.Shell.Services;
using MyShop.Shell.Views;
using System;
using System.Reflection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop.Shell
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static Window? MainWindow { get; private set; }

        public IServiceProvider Services { get; private set; }
        public new static App Current => (App)Application.Current;

        public App()
        {
            InitializeComponent();

            var services = new ServiceCollection();
            var navService = new NavigationService();

            services.AddSingleton<INavigationService>(navService);
            services.AddSingleton(navService);
            services.AddTransient<MainWindow>();
            services.AddTransient<IFilePickerService, FilePickerService>();
            services.AddTransient<IDialogService, DialogService>();
            services.AddSingleton<IUserSessionService, UserSessionService>();
            services.AddTransient<IImportService, ImportService>();
            services.AddSingleton<ILocalSettingService, LocalSettingService>();
            services.AddTransient<ICompositeMetaDataProvider, CompositeMetadataProvider>();

            services.AddClientInfrastructure();

            navService.Configure("MainLayoutPage", typeof(MainLayoutPage));

            //string pluginPath = Path.Combine(AppContext.BaseDirectory, "Plugins");
            string pluginPath = AppContext.BaseDirectory;
            PluginLoader.LoadPlugins(services, navService, pluginPath);

            Services = services.BuildServiceProvider();

            ServiceHelper.Current = Services;
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            MainWindow = App.GetService<MainWindow>();

            var authService = App.GetService<IAuthenticationService>() as AuthenticationService;
            var userSession = App.GetService<IUserSessionService>() as UserSessionService;
            
            if(authService is not null)
            {
                await authService.InitializeAsync();
            }

            var navService = App.GetService<INavigationService>() as NavigationService;
            if (MainWindow is MainWindow mainWin)
            {
                navService?.SetFrame(mainWin.MainFrame);
            }

            MainWindow.Activate();

            if(userSession is not null && userSession.IsLoggedIn)
            {
                navService?.NavigateTo("MainLayoutPage");
            }
            else
            {
                navService?.NavigateTo("LoginPage");
            }
        }

        public static T GetService<T>() where T : class
        {
            if (App.Current.Services.GetRequiredService(typeof(T)) is not T service)
            {
                throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
            }
            return service;
        }

        public void AddPluginMetadata(IXamlMetadataProvider pluginProvider)
        {
            if (pluginProvider == null) return;

            var field = this.GetType().GetField("_provider", BindingFlags.NonPublic | BindingFlags.Instance);

            if (field != null)
            {
                var currentProvider = field.GetValue(this) as IXamlMetadataProvider;

                var composite = currentProvider as CompositeMetadataProvider;

                if (composite == null)
                {
                    composite = new CompositeMetadataProvider();

                    if (currentProvider == null)
                    {
                        currentProvider = new Microsoft.UI.Xaml.XamlTypeInfo.XamlControlsXamlMetaDataProvider();
                    }

                    composite.AddProvider(currentProvider);

                    field.SetValue(this, composite);
                }

                composite.AddProvider(pluginProvider);

                System.Diagnostics.Debug.WriteLine($"---> Đã Inject Plugin Metadata thành công: {pluginProvider}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("---> CẢNH BÁO: Không tìm thấy field _provider trong App.");
            }
        }
    }
}
