using CommunityToolkit.Mvvm.ComponentModel;
using Database.models;

namespace MyShop.ViewModels
{
    public partial class ProductDetailsViewModel : ObservableObject
    {
        [ObservableProperty]
        private Product _product;

        public ProductDetailsViewModel(Product product)
        {
            Product = product;
        }

        public ProductDetailsViewModel()
        {

        }
    }
}
