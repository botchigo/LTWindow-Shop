using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MyShop.Modules.Products.Models
{
    public partial class ProductEditorModel : ObservableObject
    {
        public int Id { get; set; }

        [ObservableProperty] private string _name;
        [ObservableProperty] private string _description;
        [ObservableProperty] private decimal _importPrice;
        [ObservableProperty] private decimal _salePrice;
        [ObservableProperty] private int _stock;
        [ObservableProperty] private int _categoryId;

        public ObservableCollection<string> Images { get; set; } = new ObservableCollection<string>();
    }
}
