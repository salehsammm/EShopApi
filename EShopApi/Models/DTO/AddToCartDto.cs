namespace EShopApi.Models.DTO
{
    public class AddToCartDto
    {
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
    }
}
