using Database.models;
using MyShop.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyShop.Services
{
    public interface IDialogService
    {
        Task ShowProductDetailsDialogAsync(Product product, Action onCofirmDelete, Action onCofirmEdit);
        Task<bool> ShowConfirmAsync(string title, string message);
        Task ShowMessageAsync(string title, string message);
        Task<bool> ShowProductCreateUpdateDialogAsync(Product product);
        Task<bool> ShowImportPreviewDialogAsync(List<Product> importedProducts);
        Task ShowErrorDialogAsync(string title, string message);
        Task ShowCategoryManagementAsync();
        Task<bool> ShowOrderDetailsAsync(OrderDetailsViewModel viewModel);
    }
}
