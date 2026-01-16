using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MyShop.Contract;
using MyShop.Core.DTOs;
using MyShop.Core.Interfaces;
using MyShop.Infrastructure;
using MyShop.Modules.Products.Models;
using StrawberryShake;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyShop.Modules.Products.ViewModels
{
    public partial class ProductEditorViewModel : ObservableObject, INavigationAware
    {
        private readonly IMyShopClient _client;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly IFilePickerService _filePickerService;

        [ObservableProperty] private ProductEditorModel _product = new();
        [ObservableProperty] private string _pageTitle = "Thêm sản phẩm";
        [ObservableProperty] private bool _isLoading;

        public ObservableCollection<IGetCategoryLookup_Categories_Items> Categories { get; } = new();
        [ObservableProperty] private IGetCategoryLookup_Categories_Items? _selectedCategory;

        private List<IGetProductDetails_ProductById_ProductImages> _originalImages = new();

        public ProductEditorViewModel(IMyShopClient client, INavigationService nav, IDialogService dialog,
            IFilePickerService filePickerService)
        {
            _client = client;
            _navigationService = nav;
            _dialogService = dialog;
            _filePickerService = filePickerService;
        }

        // ======= NAVIGATION LOGIC =======
        public void OnNavigatedTo(object parameter)
        {
            _ = LoadCategoriesAsync();

            if (parameter is int productId && productId > 0)
            {
                //update mode
                PageTitle = "Cập nhật sản phẩm";
                _ = LoadProductForEditAsync(productId);
            }
            else
            {
                PageTitle = "Thêm sản phẩm mới";
                Product = new ProductEditorModel();
                SelectedCategory = Categories.FirstOrDefault();
            }
        }

        // ======= LOAD DATA =======
        private async Task LoadCategoriesAsync()
        {
            var result = await _client.GetCategoryLookup.ExecuteAsync(skip: 0, take: 30);
            if (!result.IsErrorResult())
            {
                Categories.Clear();
                foreach (var item in result.Data.Categories.Items)
                {
                    Categories.Add(item);
                }
            }
        }

        private async Task LoadProductForEditAsync(int id)
        {
            IsLoading = true;
            try
            {
                var result = await _client.GetProductDetails.ExecuteAsync(id);

                if (result.IsErrorResult())
                {
                    await _dialogService.ShowErrorDialogAsync("Lỗi", "Không tải được dữ liệu sản phẩm.");
                    _navigationService.GoBack();
                    return;
                }

                var data = result.Data.ProductById;

                Product = new ProductEditorModel()
                {
                    Id = data.Id,
                    Name = data.Name,
                    Description = data.Description,
                    ImportPrice = data.ImportPrice,
                    SalePrice = data.SalePrice,
                    Stock = data.Stock,
                    CategoryId = data.Category?.Id ?? 0
                };

                _originalImages.Clear();

                foreach (var img in data.ProductImages)
                {
                    Product.Images.Add(img.Path);
                    _originalImages.Add(img);
                }

                if (data.Category != null)
                {
                    SelectedCategory = Categories.FirstOrDefault(c => c.Id == data.Category.Id);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            //Validate
            if (string.IsNullOrWhiteSpace(Product.Name))
            {
                await _dialogService.ShowMessageAsync("Thiếu thông tin", "Tên sản phẩm không được để trống.");
                return;
            }

            if (SelectedCategory == null)
            {
                await _dialogService.ShowMessageAsync("Thiếu thông tin", "Vui lòng chọn loại sản phẩm.");
                return;
            }

            IsLoading = true;
            try
            {
                if (Product.Id == 0)
                {
                    // === CREATE ===
                    var input = new AddProductDTOInput()
                    {
                        Name = Product.Name,
                        Description = Product.Description,
                        ImportPrice = Product.ImportPrice,
                        SalePrice = Product.SalePrice,
                        Stock = Product.Stock,
                        CategoryId = SelectedCategory.Id,
                        ImagePaths = Product.Images
                    };

                    var result = await _client.AddProduct.ExecuteAsync(input);
                    if (HandleResult(result.IsErrorResult(), result.Errors))
                    {
                        if (result.Data.AddProduct.Errors is not null)
                        {
                            await _dialogService.ShowErrorDialogAsync("Thêm thất bại", result.Data.AddProduct.Errors[0].Message);
                            return;
                        }

                        await _dialogService.ShowMessageAsync("Thành công", "Sản phẩm đã được thêm!");
                        Cancel();
                    }
                }
                else
                {
                    //====UPDATE====

                    var originalImagePaths = _originalImages.Select(i => i.Path);
                    var newImagePaths = Product.Images.Except(originalImagePaths).ToList();
                    var deletedImagePaths = originalImagePaths.Except(Product.Images).ToList();
                    var deletedImageIds = _originalImages
                        .Where(i => deletedImagePaths.Contains(i.Path))
                        .Select(i => i.Id)
                        .ToList();

                    var input = new UpdateProductDTOInput
                    {
                        Id = Product.Id,
                        Name = Product.Name,
                        Description = Product.Description,
                        ImportPrice = Product.ImportPrice,
                        SalePrice = Product.SalePrice,
                        Stock = Product.Stock,
                        CategoryId = SelectedCategory.Id,
                        NewImagePaths = newImagePaths,
                        ImageIdToDelete = deletedImageIds,
                    };

                    var result = await _client.UpdateProduct.ExecuteAsync(input);

                    if (!HandleResult(result.IsErrorResult(), result.Errors)) return;

                    if (result.Data?.UpdateProduct?.Errors is { Count: > 0 } errors)
                    {
                        await _dialogService.ShowErrorDialogAsync("Cập nhật thất bại", errors[0].Message);
                        return;
                    }

                    await _dialogService.ShowMessageAsync("Thành công", "Đã cập nhật sản phẩm!");                   
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorDialogAsync("Lỗi hệ thống", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool HandleResult(bool isError, IReadOnlyList<IClientError> errors)
        {
            if (isError)
            {
                var msg = errors.Count > 0 ? errors[0].Message : "Thao tác thất bại.";
                _dialogService.ShowErrorDialogAsync("Lỗi Server", msg);
                return false;
            }
            return true;
        }

        [RelayCommand]
        private void Cancel()
        {
            WeakReferenceMessenger.Default.Send(new NavigateInnerPageMessage("ProductListPage", null));
        }

        // ======= IMAGE ACTIONS =======

        [RelayCommand]
        private async Task AddImageAsync()
        {
            try
            {
                var files = await _filePickerService.PickImagesAsync();

                if (files is not null && files.Any())
                {
                    // 1. Lấy đường dẫn chuẩn của Windows (C:\Users\Name\AppData\Local)
                    string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    // 2. Tạo thư mục riêng 
                    string appFolderPath = Path.Combine(localAppData, "MyWindowProject");

                    if (!Directory.Exists(appFolderPath))
                    {
                        Directory.CreateDirectory(appFolderPath);
                    }

                    // 4. Lấy reference dạng StorageFolder từ đường dẫn string
                    var storageFolder = await StorageFolder.GetFolderFromPathAsync(appFolderPath);

                    var imagesFolder = await storageFolder.CreateFolderAsync("ProductImages", CreationCollisionOption.OpenIfExists);

                    foreach (var file in files)
                    {
                        string newFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.Name)}";
                        var copiedFile = await file.CopyAsync(imagesFolder, newFileName, NameCollisionOption.GenerateUniqueName);

                        Product.Images.Add(copiedFile.Path);
                    }
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorDialogAsync("Lỗi chọn ảnh", ex.Message);
            }
        }

        [RelayCommand]
        private void RemoveImage(string path)
        {
            if (Product.Images.Contains(path))
            {
                Product.Images.Remove(path);
            }
        }
    }
}
