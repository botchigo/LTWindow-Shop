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
    public partial class CreateOrderViewModel : ObservableObject
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;

        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();
        public ObservableCollection<CartItemViewModel> Carts { get; } = new ObservableCollection<CartItemViewModel>();
        public List<PaymentMethod> PaymentMethods { get; } 
            = Enum.GetValues(typeof(PaymentMethod)).Cast<PaymentMethod>().ToList();

        [ObservableProperty]
        private string _searchKeyword;

        [ObservableProperty]
        private decimal _totalAmount;

        [ObservableProperty]
        private PaymentMethod _selectedPaymentMethod = PaymentMethod.COD;

        public CreateOrderViewModel(IOrderRepository orderRepository, IProductRepository productRepository,
            IDialogService dialogService, INavigationService navigationService)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _dialogService = dialogService;
            _navigationService = navigationService;

            LoadProducts();           
        }

        private async void LoadProducts()
        {
            Products.Clear();
            var products = await _productRepository.GetPagedProductsAsync(
                1,10,0,SearchKeyword);

            foreach (var product in products)
            {
                Products.Add(product);
            }
        }

        [RelayCommand]
        private void Search()
        {
            LoadProducts();
        }

        [RelayCommand]
        private async Task AddToCart(Product product)
        {
            if(product.Stock <= 0)
            {
                await _dialogService.ShowMessageAsync("Không thể thêm sản phẩm", "Sản phẩm này đã hết");
                return;
            }

            var existingItem = Carts.FirstOrDefault(c => c.Product.Id == product.Id);
            if(existingItem is not null)
            {
                if (existingItem.Quantity < product.Stock)
                    existingItem.Quantity++;
            }
            else
            {
                Carts.Add(new CartItemViewModel(product, RecalculateTotal));
            }

            RecalculateTotal();
        }

        [RelayCommand]
        private async Task RemoveFromCart(CartItemViewModel item)
        {
            var confirm = await _dialogService.ShowConfirmAsync
                ("Xác nhận xóa", "Bạn chắc chắn muốn xóa sản phẩm này khỏi giỏ hàng");

            if (confirm is false)
                return;

            Carts.Remove(item);
            RecalculateTotal();
        }

        [RelayCommand]
        private async Task DecreaseQuantity(CartItemViewModel item)
        {
            if(item.Quantity > 1)
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
        private void IncreaseQuantity(CartItemViewModel item)
        {
            if(item.Quantity < item.Product.Stock)
            {
                item.Quantity++;
                RecalculateTotal();
            }
        }

        [RelayCommand]
        private async Task Checkout()
        {
            if (Carts.Count == 0)
                return;

            //check user
            if(UserSession.IsLoggedIn is false)
            {
                await _dialogService.ShowErrorDialogAsync("Lỗi", "Phiên đăng nhập hết hạn.");
                return;
            }

            var newOrder = new Order()
            {
                FinalPrice = TotalAmount,
                Status = OrderStatus.Created,
                PaymentMethod = SelectedPaymentMethod,
                UserId = UserSession.UserId,
                OrderItems = Carts.Select(i => new OrderItem()
                {
                    Quantity = i.Quantity,
                    UnitSalePrice = i.Price,
                    UnitCost = i.Product.ImportPrice,
                    TotalPrice = i.Quantity * i.Price,
                    ProductId = i.Product.Id, 
                }).ToList(),
            };

            try
            {
                await _orderRepository.CreateOrderAsync(newOrder);
                Carts.Clear();
                TotalAmount = 0;
                LoadProducts();

                await _dialogService.ShowMessageAsync("Tạo đơn hàng", "Đơn hàng đã được tạo thành công");
            }
            catch(Exception ex)
            {
                await _dialogService.ShowErrorDialogAsync("Tạo đơn hàng", ex.Message);
            }
        }

        [RelayCommand]
        private void Goback()
        {
            _navigationService.GoBack();
        }

        private void RecalculateTotal()
        {
            TotalAmount = Carts.Sum(c => c.Subtotal);
        }
    }
}
