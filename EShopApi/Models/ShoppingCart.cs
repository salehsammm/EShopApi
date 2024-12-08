using System;
using System.Collections.Generic;

namespace EShopApi.Models;

public partial class ShoppingCart
{
    public Guid ShoppingCartId { get; set; }

    public bool IsFinal { get; set; }

    public DateTime CreateAt { get; set; }

    public Guid UserId { get; set; }

    public virtual ICollection<ShoppingCartItem> ShoppingCartItems { get; set; } = new List<ShoppingCartItem>();

    public virtual User User { get; set; } = null!;
}
