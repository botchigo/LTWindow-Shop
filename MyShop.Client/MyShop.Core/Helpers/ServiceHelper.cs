namespace MyShop.Core.Helpers
{
    public static class ServiceHelper
    {
        public static IServiceProvider? Current { get; set; }
        public static T? GetService<T>()
        {
            return (T?)Current?.GetService(typeof(T));
        }
    }
}
