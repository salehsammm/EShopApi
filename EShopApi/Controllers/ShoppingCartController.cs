using AutoMapper;
using EShopApi.Data;
using EShopApi.Models;
using EShopApi.Models.DTO;
using EShopApi.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShopApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingCartController : ControllerBase
    {
        private Eshop2DbContext _context;
        private readonly IMapper _mapper;

        public ShoppingCartController(Eshop2DbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpPost("add")]
        public async Task<ActionResult<IEnumerable<Product>>> AddToCart(AddToCartDto dto)
        {
            Product? product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == dto.ProductId);
            if (product == null)
            {
                return NotFound();
            }

            ShoppingCart? shoppingCart = await _context.ShoppingCarts.FirstOrDefaultAsync(s => s.UserId == dto.UserId);
            if (shoppingCart != null && shoppingCart.IsFinal == false)
            {
                ShoppingCartItem? shoppingCartItem = await _context.ShoppingCartItems.FirstOrDefaultAsync(s => s.ShoppingCartId == shoppingCart.ShoppingCartId && s.ProductId == dto.ProductId);
                if (shoppingCartItem != null)
                {
                    shoppingCartItem.Count++;
                    _context.Entry(shoppingCartItem).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    shoppingCartItem = new ShoppingCartItem()
                    {
                        ProductId = dto.ProductId,
                        Count = 1,
                        ShoppingCartId = shoppingCart.ShoppingCartId
                    };
                    await _context.ShoppingCartItems.AddAsync(shoppingCartItem);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                shoppingCart = new ShoppingCart()
                {
                    CreateAt = DateTime.Now,
                    IsFinal = false,
                    UserId = dto.UserId,
                };
                await _context.ShoppingCarts.AddAsync(shoppingCart);
                await _context.SaveChangesAsync();

                ShoppingCartItem shoppingCartItem = new ShoppingCartItem()
                {
                    Count = 1,
                    ProductId = dto.ProductId,
                    ShoppingCartId = shoppingCart.ShoppingCartId,
                };
                await _context.ShoppingCartItems.AddAsync(shoppingCartItem);
                await _context.SaveChangesAsync();
            };

            return Ok();
        }

        [HttpDelete("{shoppingCartItemId}")]
        public async Task<ActionResult<IEnumerable<Product>>> RemoveFromCart(Guid shoppingCartItemId)
        {
            ShoppingCartItem? shoppingCartItem = await _context.ShoppingCartItems
                .FirstOrDefaultAsync(s => s.ShoppingCartItemId == shoppingCartItemId);

            if (shoppingCartItem != null)
            {
                _context.ShoppingCartItems.Remove(shoppingCartItem);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<ShoppingCart>> GetShoppingCart(Guid userId)
        {
            CartResponse cartResponse = new CartResponse();
            ShoppingCart? shoppingCart = await _context.ShoppingCarts
                .Include(s => s.ShoppingCartItems)
                .ThenInclude(s => s.Product)
                .FirstOrDefaultAsync(s => s.UserId == userId && s.IsFinal == false);

            if (shoppingCart == null)
            {
                cartResponse.Status = 2;
                return Ok(cartResponse);
            }

            cartResponse.Status = 1;
            cartResponse.shoppingCartDto = _mapper.Map<ShoppingCartDto>(shoppingCart);

            ShoppingCartDto test = new ShoppingCartDto()
            {
                ShoppingCartId = shoppingCart.ShoppingCartId,
                IsFinal = shoppingCart.IsFinal,
                CreateAt = shoppingCart.CreateAt,
                ShoppingCartItems = shoppingCart.ShoppingCartItems.Select(item => new ShoppingCartItemDto
                {
                    ShoppingCartItemId = item.ShoppingCartItemId,
                    Count = item.Count,
                    ProductId = item.ProductId,
                    ProductName = item.Product?.Name,
                    ProductPrice = item.Product?.Price
                }).ToList()
            };

            if (cartResponse.shoppingCartDto == test)
            {
                Console.Write("'yeah'");
            }

            return Ok(cartResponse);
        }

    }
}
