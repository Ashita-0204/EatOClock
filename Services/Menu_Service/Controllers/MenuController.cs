using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Menu_Service.DTOs;
using Menu_Service.Interfaces;
using System.Security.Claims;

namespace Menu_Service.Controllers;

[ApiController]
[Route("api/v1/menu")]

public class MenuController : ControllerBase
{
    private readonly IMenuService _menu;
    public MenuController(IMenuService menu) => _menu = menu;

    private string OwnerId => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub") ?? string.Empty;

    // -- Categories -----------------------------------------

    /// <summary>Add a new menu category for a restaurant</summary>
    [HttpPost("category")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        try
        {
            var result = await _menu.CreateCategoryAsync(request, OwnerId);
            return Created($"/menu/category/{result.Id}", result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Get all categories (with items) for a restaurant</summary>
    [Authorize(Roles = "Customer,RestaurantOwner,Admin")]
    [HttpGet("category/{restaurantId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories(Guid restaurantId)
    {
        var result = await _menu.GetCategoriesByRestaurantAsync(restaurantId);
        return Ok(result);
    }

    // -- Items ----------------------------------------------

    /// <summary>Add a new menu item</summary>
    [HttpPost("item")]
    [Authorize(Roles = "RestaurantOwner,Admin")]
    public async Task<IActionResult> CreateItem([FromBody] CreateMenuItemRequest request)
    {
        try
        {
            var result = await _menu.CreateItemAsync(request, OwnerId);
            return Created($"/menu/item/{result.Id}", result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Update a menu item</summary>
    [Authorize(Roles = "RestaurantOwner,Admin")]
    [HttpPut("item/{itemId:guid}")]
    public async Task<IActionResult> UpdateItem(Guid itemId, [FromBody] UpdateMenuItemRequest request)
    {
        try
        {
            var result = await _menu.UpdateItemAsync(itemId, request, OwnerId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Delete a menu item</summary>
     [Authorize(Roles = "RestaurantOwner,Admin")]
    [HttpDelete("item/{itemId:guid}")]
    public async Task<IActionResult> DeleteItem(Guid itemId)
    {
        try
        {
            await _menu.DeleteItemAsync(itemId, OwnerId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>Toggle item availability on/off</summary>
    [Authorize(Roles = "RestaurantOwner,Admin")]
    [HttpPatch("item/{itemId:guid}/availability")]
    public async Task<IActionResult> ToggleAvailability(Guid itemId)
    {
        try
        {
            var result = await _menu.ToggleAvailabilityAsync(itemId, OwnerId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
