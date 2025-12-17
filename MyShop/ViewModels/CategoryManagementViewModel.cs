using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Database.models;
using Database.Repositories;
using MyShop.Services;
using System;
using System.Collections.ObjectModel;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;

namespace MyShop.ViewModels
{
    public partial class CategoryManagementViewModel : ObservableObject
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IDialogService _dialogService;

        public ObservableCollection<Category> Categories { get; } = new ObservableCollection<Category>();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddCategoryCommand))]
        private string newCategoryName;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddCategoryCommand))]
        private string newCategoryDescription;

        [ObservableProperty]
        private bool _isErrorOpen;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private string _errorTitle;

        public CategoryManagementViewModel(ICategoryRepository categoryRepository, IDialogService dialogService)
        {
            _categoryRepository = categoryRepository;
            _dialogService = dialogService;
            LoadCategories();
        }

        private async void LoadCategories()
        {
            Categories.Clear();
            var categories = await _categoryRepository.GetCategoriesAsync();
            foreach(var category in categories) 
                Categories.Add(category);
        }

        private bool CanAddCategory() => !string.IsNullOrEmpty(NewCategoryName);
        [RelayCommand(CanExecute = nameof(CanAddCategory))]
        private async Task AddCategoryAsync()
        {
            if(await _categoryRepository.IsNameExistAsync(NewCategoryName))
            {
                ShowInlineError("Không thể thêm", $"Loại '{NewCategoryName}' đã tồn tại.");
                return;
            }

            var newCategory = new Category
            {
                Name = NewCategoryName,
                Description = NewCategoryDescription,
            };

            try
            {
                await _categoryRepository.AddAsync(newCategory);
            }
            catch
            {
                ShowInlineError("Không thể thêm", $"Đã có lỗi xảy ra. Hãy thử lại.");
            }

            Categories.Add(newCategory);
            NewCategoryName = string.Empty;
            NewCategoryDescription = string.Empty;
        }

        [RelayCommand]
        private async Task DeleteCategoryAsync(Category category)
        {
            if (category is null)
                return;

            IsErrorOpen = false;

            bool isUsed = await _categoryRepository.IsCategoryUsedAsync(category.Id);
            if(isUsed)
            {
                ShowInlineError("Không thể xóa", $"Loại '{category.Name}' đang được sử dụng bởi sản phẩm khác.");
                return;
            }

            try
            {
                await _categoryRepository.DeleteAsync(category.Id);
            }
            catch
            {
                ShowInlineError("Không thể xóa", $"Đã có lỗi xảy ra. Hãy thử lại.");
            }

            Categories.Remove(category);
        }

        private void ShowInlineError(string title, string message)
        {
            ErrorTitle = title;
            ErrorMessage = message;
            IsErrorOpen = true;
        }
    }
}
