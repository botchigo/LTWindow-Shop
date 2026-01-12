using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using MyShop.Contract;
using MyShop.Core.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace MyShop.Shell.Services
{
    public static class PluginLoader
    {
        public static void LoadPlugins(IServiceCollection services, INavigationService navService, string pluginFolder)
        {
            if (!Directory.Exists(pluginFolder))
                return;

            var dllFiles = Directory.GetFiles(pluginFolder, "MyShop.Modules.*.dll");

            foreach (var file in dllFiles)
            {
                try
                {
                    //Load assembly from file
                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(file);

                    var moduleTypes = assembly.GetTypes()
                        .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                    foreach (var type in moduleTypes)
                    {
                        var module = (IModule?)Activator.CreateInstance(type);

                        module?.RegisterTypes(services);
                        module?.MapNavigation(navService);

                        var metadata = module?.GetMetadataProvider();

                        if (metadata != null && Application.Current is App myApp)
                        {
                            myApp.AddPluginMetadata(metadata);
                        }

                        System.Diagnostics.Debug.WriteLine($"---> Loaded Plugin: {type.Name}");
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"---> Lỗi Load Type trong {file}:");
                    foreach (var loaderEx in ex.LoaderExceptions)
                    {
                        if (loaderEx != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"   - {loaderEx.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Lỗi load plugin {file}: {ex.Message}");
                }
            }
        }
    }
}
