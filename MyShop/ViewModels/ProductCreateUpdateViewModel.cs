using CommunityToolkit.Mvvm.ComponentModel;
using Database.models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MyShop.ViewModels
{
    public partial class ProductCreateUpdateViewModel : ObservableObject
    {
        public Product Product { get; }

        public ObservableCollection<ProductImage> ImageCollection { get; }

        [ObservableProperty]
        private Category? _selectedCategory;

        public ObservableCollection<Category> Categories { get; }

        public ProductCreateUpdateViewModel(Product product, List<Category> categories)
        {
            Product = product;
            ImageCollection = new ObservableCollection<ProductImage>(product.ProductImages
                ?? Enumerable.Empty<ProductImage>());
            Categories = new ObservableCollection<Category>(categories);
            SelectedCategory = categories.FirstOrDefault(c => c.Id == product.CategoryId);
        }
        public void ApplyChanges()
        {
            Product.CategoryId = SelectedCategory?.Id ?? 0;
            Product.ProductImages = ImageCollection.ToList();
        }
    }
}
