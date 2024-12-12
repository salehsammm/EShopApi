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
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingCartController(Eshop2DbContext context, IAuthService authService) : ControllerBase
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
            Guid? userId = authService.GetUserIdFromJwt(token);
            if (userId == null)
                return Unauthorized();

            CartResponse cartResponse = new();
            //ShoppingCart? shoppingCart = await context.ShoppingCarts
            //    .Include(s => s.ShoppingCartItems)
            //    .ThenInclude(s => s.Product)
            //    .FirstOrDefaultAsync(s => s.UserId == userId && s.IsFinal == false);

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
                //cartResponse.shoppingCartDto = mapper.Map<ShoppingCartDto>(shoppingCart);
                cartResponse.shoppingCartDto = shoppingCart;
                return Ok(cartResponse);
            }

            cartResponse.Status = 2;
            return Ok(cartResponse);
        }

    }
}
