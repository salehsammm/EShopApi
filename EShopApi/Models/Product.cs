using System;
using System.Collections.Generic;

namespace EShopApi.Models;

public partial class Product
{
    public Guid ProductId { get; set; }

    public string Name { get; set; } = null!;

    public int Price { get; set; }

    public string? ImgUrl { get; set; }

    public string? Description { get; set; }

    public string Slug { get; set; } = null!;

    public virtual ICollection<ShoppingCartItem> ShoppingCartItems { get; set; } = new List<ShoppingCartItem>();
}
