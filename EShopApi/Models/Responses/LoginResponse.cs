namespace EShopApi.Models.Responses
{
    public class LoginResponse
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
