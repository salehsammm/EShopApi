namespace EShopApi.Models.DTO
{
    public class ShoppingCartItemDto
    {
        public Guid ShoppingCartItemId { get; set; }
        public int Count { get; set; }
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public int? ProductPrice { get; set; }
        public string? ProductImgUrl { get; set; }
        public string? ProductSlug { get; set; }
    }
}
