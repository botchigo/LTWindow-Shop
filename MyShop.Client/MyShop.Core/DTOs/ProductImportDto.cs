namespace MyShop.Core.DTOs
{
    public class ProductImportDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal ImportPrice { get; set; }
        public decimal SalePrice { get; set; }
        public int Stock { get; set; }
        public string Description { get; set; }
        public string CategoryName { get; set; }
    }
}
