using System;
using System.Collections.Generic;

namespace EShopApi.Models;

public partial class ShoppingCartItem
{
    public Guid ShoppingCartItemId { get; set; }

    public int Count { get; set; }

    public Guid ProductId { get; set; }

    public Guid ShoppingCartId { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ShoppingCart ShoppingCart { get; set; } = null!;
}
