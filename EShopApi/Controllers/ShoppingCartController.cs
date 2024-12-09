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

namespace EShopApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingCartController(Eshop2DbContext context, IMapper mapper, IAuthService authService) : ControllerBase
    {
        [HttpPost("add/{productId}")]
        public async Task<ActionResult<IEnumerable<Product>>> AddToCart(Guid productId)
        {
            string token = Request.Headers.Authorization.ToString();
            Guid userId = authService.GetUserIdFromJwt(token);

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

        [HttpGet]
        public async Task<ActionResult<ShoppingCart>> GetShoppingCart()
        {
            string token = Request.Headers.Authorization.ToString();
            if (string.IsNullOrEmpty(token)) return Unauthorized();
            string JwtToken = token.StartsWith("Bearer ") ? token[7..] : token;
            JwtSecurityTokenHandler handler = new();
            var jsonToken = handler.ReadJwtToken(JwtToken);
            string? userId = jsonToken.Payload["UserId"]?.ToString();
            if (Guid.TryParse(userId, out Guid userId2))
            {
                CartResponse cartResponse = new();
                ShoppingCart? shoppingCart = await context.ShoppingCarts
                    .Include(s => s.ShoppingCartItems)
                    .ThenInclude(s => s.Product)
                    .FirstOrDefaultAsync(s => s.UserId == userId2 && s.IsFinal == false);

                if (shoppingCart == null)
                {
                    cartResponse.Status = 2;
                    return Ok(cartResponse);
                }
                cartResponse.Status = 1;
                cartResponse.shoppingCartDto = mapper.Map<ShoppingCartDto>(shoppingCart);
                return Ok(cartResponse);
            }
            return Unauthorized();
        }

    }
}
