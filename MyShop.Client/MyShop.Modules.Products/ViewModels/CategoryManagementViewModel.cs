using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Contract;
using MyShop.Core.Interfaces;
using MyShop.Infrastructure;
using StrawberryShake;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MyShop.Modules.Products.ViewModels
{
    public partial class CategoryManagementViewModel : ObservableObject
    {
        private readonly IMyShopClient _client;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;

        public ObservableCollection<IGetCategories_Categories_Items> Categories { get; } 
            = new ObservableCollection<IGetCategories_Categories_Items>();

        //New category
        [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddCategoryCommand))] private string _newCategoryName = string.Empty;
        [ObservableProperty] private string _newCategoryDescription = string.Empty;
        [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddCategoryCommand))] private bool _isLoading;

        public CategoryManagementViewModel(IMyShopClient client, INavigationService nav, IDialogService dialog)
        {
            _client = client;
            _navigationService = nav;
            _dialogService = dialog;

            _ = LoadDataAsync();
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                var result = await _client.GetCategories.ExecuteAsync(skip: 0, take: 30);

                if (result.IsErrorResult())
                {
                    await _dialogService.ShowErrorDialogAsync("Lỗi xảy ra", result.Errors[0].Message);
                    return;
                }

                Categories.Clear();
                foreach(var item in result.Data.Categories.Items)
                {
                    Categories.Add(item);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanAddCategory() => !string.IsNullOrWhiteSpace(NewCategoryName) && !IsLoading;
        [RelayCommand(CanExecute = nameof(CanAddCategory))]
        private async Task AddCategoryAsync()
        {
            IsLoading = true;
            try
            {
                var input = new AddCategoryDTOInput
                {
                    Name = NewCategoryName,
                    Description = NewCategoryDescription,
                };

                var result = await _client.AddCategory.ExecuteAsync(input);

                if(result.IsErrorResult())
                {
                    await _dialogService.ShowErrorDialogAsync("Lỗi xảy ra", result.Errors[0].Message);
                    return;
                }

                var data = result.Data.AddCategory;
                if(data.Errors is not null)
                {
                    await _dialogService.ShowErrorDialogAsync("Lỗi xảy ra", data.Errors[0].Message);
                    return;
                }

                await _dialogService.ShowMessageAsync("Thêm thành công", $"Loại {NewCategoryName} đã được thêm");
                await LoadDataAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task DeleteCategoryAsync(int id)
        {
            var confirm = await _dialogService.ShowConfirmAsync("Xóa danh mục", "Bạn có chắc muốn xóa danh mục này?");
            if (!confirm) return;

            IsLoading = true;
            try
            {
                var result = await _client.DeleteCategory.ExecuteAsync(id);

                if (result.IsErrorResult())
                {
                    await _dialogService.ShowErrorDialogAsync("Lỗi", result.Errors[0].Message);
                }

                var data = result.Data.DeleteCategory;
                if (data.Errors is not null)
                {
                    await _dialogService.ShowErrorDialogAsync("Lỗi xảy ra", data.Errors[0].Message);
                    return;
                }

                await _dialogService.ShowMessageAsync("Xóa thành công", $"Danh mục đã được xóa");
                await LoadDataAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void GoBack() => _navigationService.GoBack();
    }
}
