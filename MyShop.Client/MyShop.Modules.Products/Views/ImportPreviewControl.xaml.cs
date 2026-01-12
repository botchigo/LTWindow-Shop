using Microsoft.UI.Xaml.Controls;
using MyShop.Core.DTOs;
using System.Collections.Generic;
using System.Collections.ObjectModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop.Modules.Products.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ImportPreviewControl : UserControl
    {
        public ObservableCollection<ProductImportDto> ProductsImported { get; private set; }
            = new ObservableCollection<ProductImportDto>();

        public ImportPreviewControl(List<ProductImportDto> products)
        {
            InitializeComponent();
            ProductsImported = new ObservableCollection<ProductImportDto>(products);
        }
    }
}
