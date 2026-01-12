using MyShop.Infrastructure;

namespace MyShop.Modules.Products.Models
{
    public class CustomCategory : IGetCategoryLookup_Categories_Items
    {
        public int Id {  get; set; }

        public string Name {  get; set; } = string.Empty;
    }
}
