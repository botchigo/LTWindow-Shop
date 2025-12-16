using Database.models;
using System.Collections.Generic;

namespace MyShop.ViewModels
{
    public class ProductCategoriesViewModel
    {
        public Product Product { get; set; }
        public List<Category> Categories { get; set; } = new List<Category>();
    }
}
