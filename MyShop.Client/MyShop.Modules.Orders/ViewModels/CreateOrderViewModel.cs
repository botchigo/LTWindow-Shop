using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MyShop.Contract;
using MyShop.Core.DTOs;
using MyShop.Core.Interfaces;
using MyShop.Infrastructure;
using MyShop.Modules.Orders.Models;
using StrawberryShake;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.Modules.Orders.ViewModels
{
    public partial class CreateOrderViewModel : ObservableObject
    {
        private readonly IMyShopClient _client;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly IUserSessionService _userSessionService;

        //Search
        [ObservableProperty] private string? _searchKeyword;
        public ObservableCollection<IGetProductForOrder_Products_Items> SearchResults { get; } = new();

        //cart
        public ObservableCollection<CartItem> Carts { get; } = new();
        [ObservableProperty] private decimal _totalAmount;

        //Payment
        public ObservableCollection<PaymentMethodOption> PaymentMethods { get; } = new();
        [ObservableProperty] private PaymentMethodOption _selectedPaymentMethod;

        [ObservableProperty] private bool _isLoading;

        public CreateOrderViewModel(IMyShopClient client, INavigationService nav, IDialogService dialog,
            IUserSessionService userSessionService)
        {
            _client = client;
            _navigationService = nav;
            _dialogService = dialog;
            _userSessionService = userSessionService;
        }

        public void InitPaymentMethods()
        {
            PaymentMethods.Clear();
            // Map Enum sang tiếng Việt
            PaymentMethods.Add(new PaymentMethodOption { DisplayName = "Tiền mặt", Value = PaymentMethod.Cod });
            PaymentMethods.Add(new PaymentMethodOption { DisplayName = "Ví điện tử Momo", Value = PaymentMethod.Momo });
            PaymentMethods.Add(new PaymentMethodOption { DisplayName = "Ví điện tử VNPay", Value = PaymentMethod.Vnpay });

            SelectedPaymentMethod = PaymentMethods.FirstOrDefault();
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            string keyword = SearchKeyword ?? string.Empty;
            var result = await _client.GetProductForOrder.ExecuteAsync(0, 10, keyword);
            if (!result.IsErrorResult())
            {
                SearchResults.Clear();
                foreach (var p in result.Data.Products.Items) SearchResults.Add(p);
            }
        }

        [RelayCommand]
        private void AddToCart(IGetProductForOrder_Products_Items product)
        {
            if (product.Stock <= 0)
                return;

            var existing = Carts.FirstOrDefault(c => c.ProductId == product.Id);
            if (existing != null)
            {
                if (existing.Quantity < product.Stock) existing.Quantity++;
            }
            else
            {
                Carts.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.SalePrice,
                    Quantity = 1,
                    MaxStock = product.Stock,
                    ImagePath = product.ProductImages.FirstOrDefault()?.Path
                });
            }
            RecalculateTotal();
        }

        [RelayCommand]
        private async Task RemoveFromCart(CartItem item)
        {
            var confirm = await _dialogService.ShowConfirmAsync
                ("Xác nhận xóa", "Bạn chắc chắn muốn xóa sản phẩm này khỏi giỏ hàng");

            if (confirm is false)
                return;

            Carts.Remove(item);
            RecalculateTotal();
        }

        [RelayCommand]
        private async Task DecreaseQuantity(CartItem item)
        {
            if (item.Quantity > 1)
            {
                item.Quantity--;
                RecalculateTotal();
            }
            else
            {
                await RemoveFromCart(item);
            }
        }

        [RelayCommand]
        private void IncreaseQuantity(CartItem item)
        {
            if (item.Quantity < item.MaxStock)
            {
                item.Quantity++;
                RecalculateTotal();
            }
        }

        private void RecalculateTotal()
        {
            TotalAmount = Carts.Sum(c => c.UnitPrice * c.Quantity);
        }

        [RelayCommand]
        private async Task CheckoutAsync()
        {
            if (!Carts.Any()) return;

            IsLoading = true;
            try
            {
                var input = new AddOrderDTOInput
                {
                    UserId = _userSessionService.UserId,
                    PaymentMethod = SelectedPaymentMethod.Value,
                    Status = OrderStatus.Created,
                    OrderItems = Carts.Select(c => new OrderItemInfoDTOInput
                    {
                        ProductId = c.ProductId,
                        Quantity = c.Quantity
                    }).ToList()
                };

                var result = await _client.CreateOrder.ExecuteAsync(input);

                if (result.IsErrorResult())
                {
                    await _dialogService.ShowErrorDialogAsync("Lỗi", result.Errors[0].Message);
                }

                var response = result.Data.CreateOrder;

                if (response.Errors != null && response.Errors.Count > 0)
                {
                    await _dialogService.ShowErrorDialogAsync("Lỗi tạo đơn", response.Errors[0].Message);
                    return;
                }

                await _dialogService.ShowMessageAsync("Thành công", "Đơn hàng đã tạo!");
                GoBack();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorDialogAsync("Lỗi", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void GoBack()
        {
            WeakReferenceMessenger.Default.Send(new NavigateInnerPageMessage("OrderManagementPage", null));
        }
    }
}
