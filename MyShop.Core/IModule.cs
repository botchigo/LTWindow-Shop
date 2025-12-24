using Microsoft.Extensions.DependencyInjection;

namespace MyShop.Core
{
    public interface IModule
    {
        //module register ViewModel/Service into shared DI containter
        void RegisterTypes(IServiceCollection services);
    }
}
