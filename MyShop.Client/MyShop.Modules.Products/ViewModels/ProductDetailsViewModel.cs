using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MyShop.Contract;
using MyShop.Core.DTOs;
using MyShop.Core.Interfaces;
using MyShop.Infrastructure;
using StrawberryShake;
using System.Threading.Tasks;

namespace MyShop.Modules.Products.ViewModels
{
    public partial class ProductDetailsViewModel : ObservableObject, INavigationAware
    {
        private readonly IMyShopClient _client;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;

        [ObservableProperty] private IGetProductDetails_ProductById? _product;
        [ObservableProperty] private bool _isLoading;

        public ProductDetailsViewModel(IMyShopClient client, INavigationService nav, IDialogService dialog)
        {
            _client = client;
            _navigationService = nav;
            _dialogService = dialog;
        }

        //load data when nativating to this page
        //public async Task OnNavigateTo(object parameter)
        //{
        //    if(parameter is int productId)
        //    {
        //        await LoadDataAsync(productId);
        //    }
        //}

        public void OnNavigatedFrom() { }

        private async Task LoadDataAsync(int id)
        {
            IsLoading = true;
            try
            {
                var result = await _client.GetProductDetails.ExecuteAsync(id);
                if (result.IsErrorResult())
                {
                    await _dialogService.ShowErrorDialogAsync("Lỗi", "Không tìm thấy sản phẩm");
                    _navigationService.GoBack();
                    return;
                }

                Product = result.Data.ProductById;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task DeleteProductAsync()
        {
            if (Product is null)
                return;

            var confirm = await _dialogService.ShowConfirmAsync("Xóa sản phẩm", $"Bạn chắc chắn muốn xóa {Product.Name}?");
            if (confirm is false)
                return;

            IsLoading = true;
            var result = await _client.DeleteProduct.ExecuteAsync(Product.Id);
            IsLoading = false;

            if(result.IsErrorResult())
            {
                await _dialogService.ShowMessageAsync("Lỗi", result.Errors[0].Message);
            }
            else
            {
                await _dialogService.ShowMessageAsync("Thành công", "Đã xóa sản phẩm.");
                _navigationService.GoBack();
            }
        }

        [RelayCommand]
        private void GoToEdit()
        {
            if (Product != null)
            {
                WeakReferenceMessenger.Default.Send(new NavigateInnerPageMessage("ProductEditorPage", Product.Id));
            }
        }

        [RelayCommand]
        private void GoBack()
        {
            WeakReferenceMessenger.Default.Send(new NavigateInnerPageMessage("ProductListPage", null));
        }

        public void OnNavigatedTo(object parameter)
        {
            if (parameter is int productId)
            {
                _ = LoadDataAsync(productId);
            }
        }
    }
}
