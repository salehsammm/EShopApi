using BCrypt.Net;

namespace EShopApi.Services
{
    public class HashPasswordService
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password); ;
        }
    }
}
