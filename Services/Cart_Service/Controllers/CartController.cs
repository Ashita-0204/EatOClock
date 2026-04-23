using System.Security.Claims;
using Cart_Service.DTOs;
using Cart_Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cart_Service.Controllers;

[ApiController]
[Route("api/v1/cart")]
[Authorize(Roles = "Customer,Admin")]
public class CartController : ControllerBase
{
    private readonly ICartService _cart;

    public CartController(ICartService cart) => _cart = cart;

    private string CustomerId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("Customer id claim missing.");

    // UC-28: View cart
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var cart = await _cart.GetCartAsync(CustomerId);
        if (cart == null) return Ok(new ApiResponse<object>(true, "Cart is empty.", null));
        return Ok(new ApiResponse<CartDto>(true, null, cart));
    }

    // UC-22: Add item (single-restaurant enforced in service)
    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddItemRequest req)
    {
        try
        {
            var cart = await _cart.AddItemAsync(CustomerId, req);
            return Ok(new ApiResponse<CartDto>(true, "Item added.", cart));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiResponse<object>(false, ex.Message, null));
        }
    }

    // UC-23: Update quantity
    [HttpPut("items/{itemId:guid}/qty")]
    public async Task<IActionResult> UpdateQty(Guid itemId, [FromBody] UpdateQtyRequest req)
    {
        try
        {
            var cart = await _cart.UpdateQtyAsync(CustomerId, itemId, req.Quantity);
            return Ok(new ApiResponse<CartDto>(true, "Quantity updated.", cart));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<object>(false, ex.Message, null));
        }
    }

    // UC-24: Remove item
    [HttpDelete("items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid itemId)
    {
        try
        {
            var cart = await _cart.RemoveItemAsync(CustomerId, itemId);
            return Ok(new ApiResponse<CartDto>(true, "Item removed.", cart));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<object>(false, ex.Message, null));
        }
    }

    // UC-25: Clear cart
    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        await _cart.ClearCartAsync(CustomerId);
        return Ok(new ApiResponse<object>(true, "Cart cleared.", null));
    }

    // UC-26: Apply promo
    [HttpPost("promo")]
    public async Task<IActionResult> ApplyPromo([FromBody] ApplyPromoRequest req)
    {
        try
        {
            var cart = await _cart.ApplyPromoAsync(CustomerId, req.PromoCode);
            return Ok(new ApiResponse<CartDto>(true, $"Promo '{req.PromoCode}' applied.", cart));
        }
        catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
        {
            return BadRequest(new ApiResponse<object>(false, ex.Message, null));
        }
    }

    // UC-27: Switch restaurant (clears cart, creates new)
    [HttpPost("switch-restaurant")]
    public async Task<IActionResult> SwitchRestaurant([FromBody] SwitchRestaurantRequest req)
    {
        var cart = await _cart.SwitchRestaurantAsync(CustomerId, req.NewRestaurantId);
        return Ok(new ApiResponse<CartDto>(true, "Switched restaurant. Cart cleared.", cart));
    }
}
