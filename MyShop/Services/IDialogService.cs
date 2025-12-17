using Database.models;
using System.Threading.Tasks;

namespace MyShop.Services
{
    public interface IDialogService
    {
        Task ShowProductDetailsDialogAsync(Product product);
        Task<bool> ShowConfirmAsync(string title, string message);
        Task ShowMessageAsync(string title, string message);
        Task<bool> ShowProductCreateUpdateDialogAsync(Product product);
    }
}
