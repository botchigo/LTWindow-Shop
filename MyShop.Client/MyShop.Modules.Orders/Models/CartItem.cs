using CommunityToolkit.Mvvm.ComponentModel;

namespace MyShop.Modules.Orders.Models
{
    public partial class CartItem : ObservableObject
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitCost { get; set; }

        [ObservableProperty] private int _quantity;
        public int MaxStock { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public decimal Subtotal => Quantity * UnitPrice;
    }
}
