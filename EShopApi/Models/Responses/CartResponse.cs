using EShopApi.Models.DTO;

namespace EShopApi.Models.Responses
{
    public class CartResponse
    {
        public int Status { get; set; }
        public ShoppingCartDto? shoppingCartDto { get; set; }
    }
}
