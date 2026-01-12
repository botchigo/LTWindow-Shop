using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MyShop.Contract;
using MyShop.Core.DTOs;
using MyShop.Core.Enums;
using MyShop.Core.Interfaces;
using MyShop.Infrastructure;
using MyShop.Modules.Products.Models;
using MyShop.Modules.Products.Views;
using StrawberryShake;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.Modules.Products.ViewModels
{
    public partial class ProductListViewModel : ObservableObject
    {
        private readonly IMyShopClient _client;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly IFilePickerService _filePickerService;
        private readonly IImportService _importService;

        public ObservableCollection<IGetProductList_Products_Items> Products { get; set; } = new();
        [ObservableProperty] private IGetProductList_Products_Items? _selectedProduct;

        //Paging
        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private int _pageSize = 14;
        [ObservableProperty] private int _totalPage = 1;
        [ObservableProperty][NotifyPropertyChangedFor(nameof(IsNotLoading))] private bool _isLoading;
        public bool IsNotLoading => !IsLoading;

        //Filter
        [ObservableProperty] private string? _keyword = "";
        [ObservableProperty] private decimal? _minPrice;
        [ObservableProperty] private decimal? _maxPrice;

        public ObservableCollection<IGetCategoryLookup_Categories_Items> Categories { get; } = new();
        [ObservableProperty] private IGetCategoryLookup_Categories_Items? _selectedCategory;

        //Sorting
        public ObservableCollection<SortCriteria> SortFields { get; } = new()
        {
            SortCriteria.Name,
            SortCriteria.Price,
            SortCriteria.CreateDate,
            SortCriteria.UpdateDate
        };

        public ObservableCollection<SortDirection> SortDirections { get; } = new()
        {
            SortDirection.Ascending,
            SortDirection.Descending
        };

        [ObservableProperty] private SortCriteria _selectedSortCriteria = SortCriteria.Name;
        [ObservableProperty] private SortDirection _selectedSortDirection = SortDirection.Ascending;

        public ProductListViewModel(IMyShopClient client, INavigationService nav, IDialogService dialog,
            IFilePickerService filePickerService, IImportService importService)
        {
            _client = client;
            _navigationService = nav;
            _dialogService = dialog;
            _filePickerService = filePickerService;
            _importService = importService;

            _ = LoadCategoriesAsync();
            _ = LoadDataAsync();
        }

        //=======AUTO RELOAD========

        public async Task RefreshDataAsync() => await LoadDataAsync();

        partial void OnSelectedCategoryChanged(IGetCategoryLookup_Categories_Items? value)
        {
            CurrentPage = 1; // Reset về trang 1
            _ = LoadDataAsync();
        }

        partial void OnSelectedSortCriteriaChanged(SortCriteria value) => _ = LoadDataAsync();

        partial void OnSelectedSortDirectionChanged(SortDirection value) => _ = LoadDataAsync();

        [RelayCommand]
        private async Task SearchAsync()
        {
            CurrentPage = 1;
            await LoadDataAsync();
        }

        //=======LOAD DATA========

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                var skip = (CurrentPage - 1) * PageSize;
                var sortInoput = BuildSortInput();
                var orderParams = new List<ProductSortInput> { sortInoput };
                int? categoryFilter = (SelectedCategory is null)
                    ? 0
                    : SelectedCategory.Id;
                var filter = new ProductFilterInput();
                if(MinPrice.HasValue || MaxPrice.HasValue)
                {
                    filter.ImportPrice = new DecimalOperationFilterInput();

                    if(MinPrice.HasValue)
                        filter.ImportPrice.Gte = MinPrice.Value;

                    if(MaxPrice.HasValue)
                        filter.ImportPrice.Lte = MaxPrice.Value;
                }
                
                var result = await _client.GetProductList.ExecuteAsync(
                    skip,
                    PageSize,
                    keyword: Keyword,
                    categoryId: categoryFilter,
                    filter: filter,
                    order: orderParams);

                if (result.IsErrorResult())
                {
                    var errorMsg = result.Errors.Count > 0 ? result.Errors[0].Message : "Lỗi không xác định";
                    await _dialogService.ShowErrorDialogAsync("Lỗi tải sản phẩm", errorMsg);
                    return;
                }

                var data = result.Data.Products;

                TotalPage = (int)Math.Ceiling((double)data.TotalCount / PageSize);

                //update UI
                Products.Clear();
                foreach (var item in data.Items)
                {
                    Products.Add(item);
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorDialogAsync("Lỗi tải sản phẩm", $"Có lỗi xảy ra khi tải danh sách sản phẩm. {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                LoadNextPageCommand.NotifyCanExecuteChanged();
                LoadPreviousPageCommand.NotifyCanExecuteChanged();
            }
        }

        private ProductSortInput BuildSortInput()
        {
            var direction = SelectedSortDirection == SortDirection.Ascending
                ? SortEnumType.Asc
                : SortEnumType.Desc;

            return SelectedSortCriteria switch
            {
                SortCriteria.Name => new ProductSortInput { Name = direction },
                SortCriteria.Price => new ProductSortInput { SalePrice = direction },
                SortCriteria.CreateDate => new ProductSortInput { CreatedAt = direction },
                SortCriteria.UpdateDate => new ProductSortInput { UpdatedAt = direction },
                _ => new ProductSortInput { Id = direction }
            };
        }

        private async Task LoadCategoriesAsync()
        {
            var result = await _client.GetCategoryLookup.ExecuteAsync(0, 30);
            if (!result.IsErrorResult())
            {
                Categories.Clear();

                Categories.Add(new CustomCategory { Id = 0, Name = "Tất cả" });

                foreach (var item in result.Data.Categories.Items)
                {
                    Categories.Add(item);
                }

                SelectedCategory = Categories.FirstOrDefault();
            }
        }

        //=======NAVIGATION & ACTIONS========

        [RelayCommand]
        private void GoToCreate()
        {
            WeakReferenceMessenger.Default.Send(new NavigateInnerPageMessage("ProductEditorPage", 0));
        }

        [RelayCommand]
        private void GoToDetails(int id)
        {
            WeakReferenceMessenger.Default.Send(new NavigateInnerPageMessage("ProductDetailsPage", id));
        }

        [RelayCommand]
        private void GoToCategryManagement()
        {
            WeakReferenceMessenger.Default.Send(new NavigateInnerPageMessage("CategoryManagementPage", null));
        }

        //=======PAGINATION========

        private bool CanLoadNextPage() => CurrentPage < TotalPage && IsLoading == false;

        [RelayCommand(CanExecute = nameof(CanLoadNextPage))]
        private async Task LoadNextPageAsync()
        {
            CurrentPage++;
            await LoadDataAsync();
        }

        private bool CanLoadPreviousPage() => CurrentPage > 1 && IsLoading == false;

        [RelayCommand(CanExecute = nameof(CanLoadPreviousPage))]
        private async Task LoadPreviousPageAsync()
        {
            CurrentPage--;
            await LoadDataAsync();
        }

        //=====IMPORT=====
        [RelayCommand]
        private async Task ImportProductsAsync()
        {
            try
            {
                var file = await _filePickerService.PickImportFileAsync();
                if (file is null)
                    return;

                IsLoading = true;

                List<ProductImportDto> importProducts = new();

                if (file.FileType.ToLower().Contains("xls"))
                {
                    importProducts = await _importService.ParseExcelAsync(file.Path);
                }
                else if (file.FileType.ToLower().Contains("db"))
                {
                    importProducts = await _importService.ParseAccessAsync(file.Path);
                }
                else
                    return;

                if (importProducts.Count == 0)
                {
                    await _dialogService.ShowErrorDialogAsync("Lỗi", "Không có sản phẩm hợp lệ");
                    return;
                }

                var preview = new ImportPreviewControl(importProducts);
                preview.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch;
                preview.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Stretch;

                var confirm = await _dialogService.ShowConfirmationDialogAsync("Bản xem trước",preview);
                if (confirm is false)
                    return;

                var graphqlInput = importProducts
                    .Select(dto => new ImportProductDTOInput
                    {
                        Name = dto.Name,
                        ImportPrice = dto.ImportPrice,
                        SalePrice = dto.SalePrice,
                        Stock = dto.Stock,
                        Description = dto.Description,
                        CategoryName = dto.CategoryName,
                    })
                    .ToList();

                var result = await _client.ImportProducts.ExecuteAsync(graphqlInput);
                if (result.IsErrorResult())
                {
                    await _dialogService.ShowErrorDialogAsync("Lỗi xảy ra", result.Errors[0].Message);
                    return;
                }

                var data = result.Data.ImportProducts;
                if (data.Errors is not null)
                {
                    await _dialogService.ShowErrorDialogAsync("Lỗi xảy ra", data.Errors[0].Message);
                    return;
                }

                await _dialogService.ShowMessageAsync("Thêm thành công", "Sản phẩm đã được thêm");
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorDialogAsync("Lỗi xảy ra", $"Lỗi hệ thống, vui lòng thử lại. {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
