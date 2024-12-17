using AutoMapper;
using EShopApi.Data;
using EShopApi.Models;
using EShopApi.Models.DTO;
using EShopApi.Models.Responses;
using EShopApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EShopApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingCartController(Eshop2DbContext context) : ControllerBase
    {
        [HttpPost("add/{productId}")]
        public async Task<ActionResult<IEnumerable<Product>>> AddToCart(Guid productId)
        {
            var userClaim = HttpContext.User;
            Claim? userIdClaim = userClaim.Claims.FirstOrDefault(c => c.Type == "UserId");
            Guid userId = Guid.Empty;
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out userId))
            {
                Product? product = await context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
                if (product == null)
                    return NotFound();

                ShoppingCart? shoppingCart = await context.ShoppingCarts.FirstOrDefaultAsync(s => s.UserId == userId);
                if (shoppingCart != null && shoppingCart.IsFinal == false)
                {
                    ShoppingCartItem? shoppingCartItem = await context.ShoppingCartItems
                        .FirstOrDefaultAsync(s => s.ShoppingCartId == shoppingCart.ShoppingCartId && s.ProductId == productId);
                    if (shoppingCartItem != null)
                    {
                        shoppingCartItem.Count++;
                        context.Entry(shoppingCartItem).State = EntityState.Modified;
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        shoppingCartItem = new ShoppingCartItem()
                        {
                            ProductId = productId,
                            Count = 1,
                            ShoppingCartId = shoppingCart.ShoppingCartId
                        };
                        await context.ShoppingCartItems.AddAsync(shoppingCartItem);
                        await context.SaveChangesAsync();
                    }
                }
                else
                {
                    shoppingCart = new ShoppingCart()
                    {
                        CreateAt = DateTime.Now,
                        IsFinal = false,
                        UserId = userId,
                    };
                    await context.ShoppingCarts.AddAsync(shoppingCart);
                    await context.SaveChangesAsync();

                    ShoppingCartItem shoppingCartItem = new()
                    {
                        Count = 1,
                        ProductId = productId,
                        ShoppingCartId = shoppingCart.ShoppingCartId,
                    };
                    await context.ShoppingCartItems.AddAsync(shoppingCartItem);
                    await context.SaveChangesAsync();
                }
                return Ok();
            }
            return Unauthorized();
        }

        [HttpDelete("{shoppingCartItemId}")]
        public async Task<ActionResult<IEnumerable<Product>>> RemoveFromCart(Guid shoppingCartItemId)
        {
            ShoppingCartItem? shoppingCartItem = await context.ShoppingCartItems
                .FirstOrDefaultAsync(s => s.ShoppingCartItemId == shoppingCartItemId);

            if (shoppingCartItem != null)
            {
                context.ShoppingCartItems.Remove(shoppingCartItem);
                await context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }

        [HttpDelete("remove/{shoppingCartItemId}")]
        public async Task<ActionResult<IEnumerable<Product>>> RemoveCountFromCart(Guid shoppingCartItemId)
        {
            ShoppingCartItem? shoppingCartItem = await context.ShoppingCartItems
                .FirstOrDefaultAsync(s => s.ShoppingCartItemId == shoppingCartItemId);

            if (shoppingCartItem != null)
            {
                if (shoppingCartItem?.Count > 1)
                {
                    shoppingCartItem.Count--;
                    context.Entry(shoppingCartItem).State = EntityState.Modified;
                    await context.SaveChangesAsync();
                }
                else if (shoppingCartItem?.Count == 1)
                {
                    context.ShoppingCartItems.Remove(shoppingCartItem);
                    await context.SaveChangesAsync();
                }

                return Ok();
            }



            return NotFound();
        }

        [HttpGet]
        public async Task<ActionResult<ShoppingCart>> GetShoppingCart()
        {
            var userClaim = HttpContext.User;
            Claim? userIdClaim = userClaim.Claims.FirstOrDefault(c => c.Type == "UserId");
            Guid userId = Guid.Empty;
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out userId))
            {
                CartResponse cartResponse = new();
                ShoppingCartDto? shoppingCart = await context.ShoppingCarts
                    .Where(s => s.UserId == userId && s.IsFinal == false)
                    .Select(s => new ShoppingCartDto
                    {
                        CreateAt = s.CreateAt,
                        IsFinal = s.IsFinal,
                        ShoppingCartId = s.ShoppingCartId,
                        ShoppingCartItems = s.ShoppingCartItems
                            .Select(item => new ShoppingCartItemDto
                            {
                                ShoppingCartItemId = item.ShoppingCartItemId,
                                Count = item.Count,
                                ProductId = item.ProductId,
                                ProductName = item.Product.Name,
                                ProductPrice = item.Product.Price,
                                ProductImgUrl = item.Product.ImgUrl,
                                ProductSlug = item.Product.Slug
                            }).ToList()
                    }).FirstOrDefaultAsync();

                if (shoppingCart != null)
                {
                    cartResponse.Status = 1;
                    cartResponse.shoppingCartDto = shoppingCart;
                    return Ok(cartResponse);
                }

                cartResponse.Status = 2;
                return Ok(cartResponse);
            }
            return Unauthorized();
        }

        [HttpGet("count")]
        public async Task<ActionResult<int>> GetShoppingCartCount()
        {
            var userClaim = HttpContext.User;
            Claim? userIdClaim = userClaim.Claims.FirstOrDefault(c => c.Type == "UserId");
            Guid userId = Guid.Empty;
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out userId))
            {
                int itemCount = 0;
                ShoppingCart? shoppingCart = await context.ShoppingCarts.Include(s => s.ShoppingCartItems)
                     .FirstOrDefaultAsync(s => s.UserId == userId && s.IsFinal == false);

                if (shoppingCart != null)
                {
                    itemCount = shoppingCart.ShoppingCartItems.Count;
                }
                return Ok(itemCount);
            }
            return Unauthorized();
        }

    }
}
