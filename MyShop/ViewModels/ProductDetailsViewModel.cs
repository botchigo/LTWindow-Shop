using Database.models;

namespace MyShop.ViewModels
{
    public class ProductDetailsViewModel
    {
        public Product Product { get;}
        public ProductDetailsViewModel(Product product)
        {
            Product = product;
        }
    }
}
