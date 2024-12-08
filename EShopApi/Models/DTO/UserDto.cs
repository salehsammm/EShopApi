namespace EShopApi.Models.DTO
{
    public class UserDto
    {
        public Guid UserId { get; set; }

        public string UserName { get; set; } = null!;

        public string? FName { get; set; }

        public string? LName { get; set; }

        public string? PhoneNumber { get; set; }

        public bool IsAdmin { get; set; }
    }
}
