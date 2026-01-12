using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Markup;

namespace MyShop.Contract
{
    public interface IModule
    {
        //module register ViewModel/Service into shared DI containter
        void RegisterTypes(IServiceCollection services);
        void MapNavigation(INavigationService navService);
        IXamlMetadataProvider GetMetadataProvider();
    }
}
