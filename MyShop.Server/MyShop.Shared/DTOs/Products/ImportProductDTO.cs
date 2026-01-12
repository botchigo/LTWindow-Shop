namespace MyShop.Shared.DTOs.Products
{
    public record ImportProductDTO : ProductBaseDTO
    {
        public string CategoryName { get; init; } = string.Empty;
    }
}
