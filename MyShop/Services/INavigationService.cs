namespace MyShop.Services
{
    public interface INavigationService
    {
        void NavigateTo<T>(object? parameter = null); 
        void GoBack();
    }
}
