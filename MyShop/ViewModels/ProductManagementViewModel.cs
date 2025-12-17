using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Database.Enums;
using Database.models;
using Database.Repositories;
using MyShop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MyShop.ViewModels
{
    public partial class ProductManagementViewModel : ObservableObject
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IDialogService _dialogService;
        private const int _pageSize = 10;

        //data
        [ObservableProperty]
        private ObservableCollection<Product> _filteredProducts;

        [ObservableProperty]
        private ObservableCollection<Category> _categories;

        [ObservableProperty]
        private Product? _selectedProduct;

        //Paging
        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _totalPage = 1;

        [ObservableProperty]
        private bool _isLoading = false;

        //filter
        [ObservableProperty]
        private Category? _selectedCategory;

        [ObservableProperty]
        private string? _keyword;

        [ObservableProperty]
        private decimal? _minPrice;

        [ObservableProperty]
        private decimal? _maxPrice;

        //Sorting
        [ObservableProperty]
        private ObservableCollection<SortCriteria> _sortCriterias;

        [ObservableProperty]
        private ObservableCollection<SortDirection> _sortDirections;

        [ObservableProperty]
        private SortCriteria _selectedSortCriteria = SortCriteria.Default;

        [ObservableProperty]
        private SortDirection _selectedSortDirection = SortDirection.Ascending;

        [ObservableProperty]
        private string _selectedSortOption;

        public ProductManagementViewModel(ICategoryRepository categoryRepository, IProductRepository productRepository
            , IDialogService dialogService)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
            _dialogService = dialogService;

            _filteredProducts = new ObservableCollection<Product>();
            _categories = new ObservableCollection<Category>();
            _sortCriterias = new ObservableCollection<SortCriteria>();
            _sortDirections = new ObservableCollection<SortDirection>();

            LoadSortOptions();
        }

        //page command 
        private async Task LoadCurrentPageAsync()
        {
            IsLoading = true;
            try
            {
                var pagedProducts = await _productRepository.GetPagedProductsAsync(
                    CurrentPage, _pageSize,
                    SelectedCategory?.Id ?? 0, Keyword, MinPrice, MaxPrice,
                    SelectedSortCriteria, SelectedSortDirection);

                FilteredProducts.Clear();
                foreach (var pageProduct in pagedProducts)
                {
                    FilteredProducts.Add(pageProduct);
                }
            }
            finally
            {
                IsLoading = false;
                LoadNextPageCommand.NotifyCanExecuteChanged();
                LoadPreviousPageCommand.NotifyCanExecuteChanged();
            }
        }

        private async Task CalculateTotalPageAndLoadPageAsync()
        {
            IsLoading = true;
            try
            {
                var categoryId = SelectedCategory?.Id ?? 0;
                var totalAmount = await _productRepository
                    .GetTotalProductAmountAsync(categoryId, Keyword, MinPrice, MaxPrice);
                TotalPage = (int)Math.Ceiling((double)totalAmount / _pageSize);

                if (CurrentPage > TotalPage && TotalPage > 0)
                    CurrentPage = TotalPage;
                else if (TotalPage == 0)
                    CurrentPage = 1;

                await LoadCurrentPageAsync();
            }
            finally
            {
                IsLoading = false;
                LoadNextPageCommand.NotifyCanExecuteChanged();
                LoadPreviousPageCommand.NotifyCanExecuteChanged();
            }
        }

        //pagination implementations
        private bool CanLoadNextPage() => CurrentPage < TotalPage && IsLoading == false;

        [RelayCommand(CanExecute = nameof(CanLoadNextPage))]
        private async Task LoadNextPageAsync()
        {
            CurrentPage++;
            await LoadCurrentPageAsync();
        }

        private bool CanLoadPreviousPage() => CurrentPage > 1 && IsLoading == false;

        [RelayCommand(CanExecute = nameof(CanLoadPreviousPage))]
        private async Task LoadPreviousPageAsync()
        {
            CurrentPage--;
            await LoadCurrentPageAsync();
        }

        //search command
        public async Task LoadInitalDataAndFilterAsync()
        {
            await LoadCagoriesAsync();
            await ReloadDataAsync();
        }

        [RelayCommand]
        private async Task ReloadDataAsync()
        {
            CurrentPage = 1;
            await CalculateTotalPageAndLoadPageAsync();
        }

        //delete command
        [RelayCommand]
        private async Task DeleteProductAsync(Product? product)
        {
            var targetProduct = product ?? SelectedProduct;

            if (targetProduct is null)
                return;

            //confirm dialog
            var isDeleted = await _dialogService.ShowConfirmAsync(
                $"Xác nhận xóa sản phẩm {targetProduct.Name}",
                "Bạn có chắc chắn muốn xóa sản phẩm này không?");

            if (isDeleted is false)
                return;

            await _productRepository.DeleteProductAsync(targetProduct.Id);

            //notify dialog
            await _dialogService.ShowMessageAsync("Xóa sản phẩm", "Sản phẩm đã được xóa thành công!");

            await ReloadDataAsync();
        }

        //create command
        [RelayCommand]
        private async Task CreateProductAsync()
        {
            var newProduct = new Product
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            bool isSaved = await _dialogService.ShowProductCreateUpdateDialogAsync(newProduct);

            if (isSaved)
            {
                await _productRepository.AddProductAsync(newProduct);

                //notify dialog
                await _dialogService.ShowMessageAsync(
                    "Thêm sản phẩm",
                    $"Sản phẩm {newProduct.Name} đã được thêm thành công!");

                await ReloadDataAsync();
            }
        }

        //update command
        [RelayCommand]
        private async Task UpdateProductAsync(Product? product)
        {
            if (product is null)
                return;

            var cloneProduct = new Product
            {
                Id = product.Id,
                Name = product.Name,
                ImportPrice = product.ImportPrice,
                SalePrice = product.SalePrice,
                Description = product.Description,
                CategoryId = product.CategoryId,
                Category = product.Category,
                ProductImages = new List<ProductImage>(product.ProductImages)
            };

            bool isSaved = await _dialogService.ShowProductCreateUpdateDialogAsync(cloneProduct);

            if (isSaved)
            {
                await _productRepository.UpdateProductAsync(cloneProduct);

                await _dialogService.ShowMessageAsync(
                    "Cập nhật sản phẩm",
                    $"Sản phẩm {cloneProduct.Name} đã được cập nhật thành công!");

                await ReloadDataAsync();
            }
        }

        [RelayCommand]
        private async Task ViewProductAsync(Product? product)
        {
            if (product is null)
                return;

            await _dialogService.ShowProductDetailsDialogAsync(product);
        }

        //Change handlers
        partial void OnSelectedCategoryChanged(Category? oldValue, Category? newValue)
        {
            if (oldValue != newValue)
                _ = ReloadDataAsync();
        }

        partial void OnSelectedSortCriteriaChanged(SortCriteria value)
        {
            _ = ReloadDataAsync();
        }

        partial void OnSelectedSortDirectionChanged(SortDirection value)
        {
            _ = ReloadDataAsync();
        }

        //Setup data
        private async Task LoadCagoriesAsync()
        {
            Categories.Clear();
            Categories.Add(new Category() { Id = 0, Name = "All" });
            var categories = await _categoryRepository.GetCategoriesAsync();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }
            SelectedCategory = Categories.FirstOrDefault()!;
        }

        private void LoadSortOptions()
        {
            //Sorting criteria
            SortCriterias.Add(SortCriteria.Default);
            SortCriterias.Add(SortCriteria.Name);
            SortCriterias.Add(SortCriteria.Price);
            SortCriterias.Add(SortCriteria.CreateDate);
            SortCriterias.Add(SortCriteria.UpdateDate);
            SelectedSortCriteria = SortCriteria.Default;

            //sorting direction
            SortDirections.Add(SortDirection.Ascending);
            SortDirections.Add(SortDirection.Descending);
            SelectedSortDirection = SortDirection.Ascending;
        }
    }
}
