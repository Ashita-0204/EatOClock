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
    private readonly ILogger<CartController> _logger;

    public CartController(ICartService cart, ILogger<CartController> logger)
    {
        _cart = cart;
        _logger = logger;
    }

    // FIX: Wrap in a try/catch that returns 401 when the claim is missing,
    // instead of letting UnauthorizedAccessException bubble into a 500.
    private string GetCustomerId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub")          // some JWT issuers use "sub"
               ?? User.FindFirstValue("nameid");       // legacy claim type

        if (string.IsNullOrWhiteSpace(id))
            throw new UnauthorizedAccessException("Customer ID claim is missing from the token.");

        return id;
    }

    // View cart
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        try
        {
            var cart = await _cart.GetCartAsync(GetCustomerId());
            if (cart == null) return Ok(new ApiResponse<object>(true, "Cart is empty.", null));
            return Ok(new ApiResponse<CartDTOs>(true, null, cart));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ApiResponse<object>(false, ex.Message, null));
        }
    }

    // Add item (single-restaurant enforced in service)
    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddItemRequest req)
    {
        try
        {
            var cart = await _cart.AddItemAsync(GetCustomerId(), req);
            return Ok(new ApiResponse<CartDTOs>(true, "Item added.", cart));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ApiResponse<object>(false, ex.Message, null));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Conflict in AddItem: {Message}", ex.Message);
            return Conflict(new ApiResponse<object>(false, ex.Message, null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CRITICAL ERROR in AddItem");
            return StatusCode(500, new ApiResponse<object>(false, $"Internal error: {ex.Message}", null));
        }
    }

    // Update quantity
    [HttpPut("items/{itemId:guid}/qty")]
    public async Task<IActionResult> UpdateQty(Guid itemId, [FromBody] UpdateQtyRequest req)
    {
        try
        {
            var cart = await _cart.UpdateQtyAsync(GetCustomerId(), itemId, req.Quantity);
            return Ok(new ApiResponse<CartDTOs>(true, "Quantity updated.", cart));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ApiResponse<object>(false, ex.Message, null));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<object>(false, ex.Message, null));
        }
    }

    // Remove item
    [HttpDelete("items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid itemId)
    {
        try
        {
            var cart = await _cart.RemoveItemAsync(GetCustomerId(), itemId);
            return Ok(new ApiResponse<CartDTOs>(true, "Item removed.", cart));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ApiResponse<object>(false, ex.Message, null));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<object>(false, ex.Message, null));
        }
    }

    // Clear cart
    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        try
        {
            await _cart.ClearCartAsync(GetCustomerId());
            return Ok(new ApiResponse<object>(true, "Cart cleared.", null));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ApiResponse<object>(false, ex.Message, null));
        }
    }

    // Apply promo
    [HttpPost("promo")]
    public async Task<IActionResult> ApplyPromo([FromBody] ApplyPromoRequest req)
    {
        try
        {
            var cart = await _cart.ApplyPromoAsync(GetCustomerId(), req.PromoCode);
            return Ok(new ApiResponse<CartDTOs>(true, $"Promo '{req.PromoCode}' applied.", cart));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ApiResponse<object>(false, ex.Message, null));
        }
        catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
        {
            return BadRequest(new ApiResponse<object>(false, ex.Message, null));
        }
    }

    // Switch restaurant (clears cart, creates new)
    [HttpPost("switch-restaurant")]
    public async Task<IActionResult> SwitchRestaurant([FromBody] SwitchRestaurantRequest req)
    {
        try
        {
            var cart = await _cart.SwitchRestaurantAsync(GetCustomerId(), req.NewRestaurantId);
            return Ok(new ApiResponse<CartDTOs>(true, "Switched restaurant. Cart cleared.", cart));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ApiResponse<object>(false, ex.Message, null));
        }
    }
}