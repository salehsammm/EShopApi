
namespace EShopApi.Models;

public partial class User
{
    public Guid UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Fname { get; set; }

    public string? Lname { get; set; }

    public string? PhoneNumber { get; set; }

    public bool IsAdmin { get; set; }

    public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();
}
