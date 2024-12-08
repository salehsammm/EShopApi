namespace EShopApi.Models.DTO
{
    public class RemoveFromCartDto
    {
        public Guid UserId { get; set; }
        public Guid ItemId { get; set; }
    }
}
