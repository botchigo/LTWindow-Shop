using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Database.models;
using System;

namespace MyShop.ViewModels
{
    public partial class CartItemViewModel : ObservableObject
    {
        public Product Product { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Subtotal))]
        private int _quantity;

        private readonly Action? _onQuantityChanged;

        public decimal Price => Product.SalePrice;
        public decimal Subtotal => Price * Quantity;

        public CartItemViewModel(Product product,Action? onQuantityChanged = null, int quantity = 1)
        {
            Product = product;
            Quantity = quantity;
            _onQuantityChanged = onQuantityChanged;
        }

        [RelayCommand]
        private void Increase()
        {
            if(Quantity < Product.Stock)
            {
                Quantity++;
                _onQuantityChanged?.Invoke();
            }
        }

        [RelayCommand]
        private void Decrease()
        {
            if(Quantity > 1)
            {
                Quantity--;
                _onQuantityChanged?.Invoke();
            }
        }
    }
}
