using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant_Service.DTOs;
using Restaurant_Service.Interfaces;

namespace Restaurant_Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RestaurantController : ControllerBase
{
    private readonly IRestaurantService _restaurantService;

    public RestaurantController(IRestaurantService restaurantService)
    {
        _restaurantService = restaurantService;
    }

    [HttpPost]
    [Authorize(Roles = "RestaurantOwner,Admin")]
    public async Task<IActionResult> CreateRestaurant([FromBody] CreateRestaurantDTO dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var restaurant = await _restaurantService.CreateRestaurantAsync(dto, userId);
        return CreatedAtAction(nameof(GetRestaurantById), new { id = restaurant!.Id }, restaurant);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRestaurantById(Guid id)
    {
        var restaurant = await _restaurantService.GetRestaurantByIdAsync(id);
        if (restaurant == null)
            return NotFound();

        return Ok(restaurant);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRestaurants([FromQuery] bool includeUnapproved = false)
    {
        var restaurants = await _restaurantService.GetAllRestaurantsAsync(includeUnapproved);
        return Ok(restaurants);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchRestaurants([FromQuery] SearchRestaurantDTO dto)
    {
        var restaurants = await _restaurantService.SearchRestaurantsAsync(dto);
        return Ok(restaurants);
    }

    [HttpPost("nearby")]
    public async Task<IActionResult> GetNearbyRestaurants([FromBody] NearbySearchDTO dto)
    {
        var restaurants = await _restaurantService.GetNearbyRestaurantsAsync(dto);
        return Ok(restaurants);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "RestaurantOwner,Admin")]
    public async Task<IActionResult> UpdateRestaurant(Guid id, [FromBody] UpdateRestaurantDTO dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var restaurant = await _restaurantService.UpdateRestaurantAsync(id, dto, userId);
        if (restaurant == null)
            return NotFound();

        return Ok(restaurant);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "RestaurantOwner,Admin")]
    public async Task<IActionResult> DeleteRestaurant(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _restaurantService.DeleteRestaurantAsync(id, userId);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveRestaurant(Guid id)
    {
        var result = await _restaurantService.ApproveRestaurantAsync(id);
        if (!result)
            return NotFound();

        return Ok(new { message = "Restaurant approved successfully" });
    }

    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RejectRestaurant(Guid id)
    {
        var result = await _restaurantService.RejectRestaurantAsync(id);
        if (!result)
            return NotFound();

        return Ok(new { message = "Restaurant rejected" });
    }

    [HttpGet("owner/my-restaurants")]
    [Authorize(Roles = "RestaurantOwner,Admin")]
    public async Task<IActionResult> GetMyRestaurants()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var restaurants = await _restaurantService.GetRestaurantsByOwnerAsync(userId);
        return Ok(restaurants);
    }
}
