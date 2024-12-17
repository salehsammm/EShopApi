namespace EShopApi.Models.DTO
{
    public class ShoppingCartDto
    {
        public Guid ShoppingCartId { get; set; }
        public bool IsFinal { get; set; }
        public DateTime CreateAt { get; set; }
        public int TotalPrice
        {
            get
            {
                return ShoppingCartItems.Sum(s => (s.ProductPrice ?? 0) * s.Count);
            }
        }
        public List<ShoppingCartItemDto> ShoppingCartItems { get; set; } = [];
    }
}
