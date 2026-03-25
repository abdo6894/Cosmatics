
using Cosmatics.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Cosmatics.Infrastructure.Services;

namespace Cosmatics.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        return Ok(await _cartService.GetCartAsync(GetUserId()));
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToCart(int productId, int quantity)
    {
        await _cartService.AddToCartAsync(GetUserId(), productId, quantity);
        return Ok(new { message = "Item added to cart." });
    }

    [HttpDelete("remove/{productId}")]
    public async Task<IActionResult> RemoveFromCart(int productId)
    {
        await _cartService.RemoveFromCartAsync(GetUserId(), productId);
        return Ok(new { message = "Item removed from cart." });
    }

    [HttpDelete("user/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ClearUserCart(int userId)
    {
        await _cartService.ClearCartAsync(userId);
        return Ok(new { message = "User cart cleared." });
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateCartItem(int productId, int quantity)
    {
        var result = await _cartService.UpdateCartItemQuantityAsync(GetUserId(), productId, quantity);
        if (!result) return NotFound(new { message = "Item not found in cart." });
        return Ok(new { message = "Cart item updated." });
    }

    [HttpGet("user/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserCart(int userId)
    {
        return Ok(await _cartService.GetCartAsync(userId));
    }
}
