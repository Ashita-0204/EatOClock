using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DeliveryAgent_Service.DTOs;
using DeliveryAgent_Service.Interfaces;

namespace DeliveryAgent_Service.Controllers;

[ApiController]
[Route("api/v1/agents")]
public class AgentController(IAgentService svc) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // ── UC-43: Register ───────────────────────────────────────────────────────
    [HttpPost("register")]
    [Authorize(Roles = "DeliveryAgent,Admin")]
    public async Task<IActionResult> Register([FromBody] RegisterAgentRequest req)
    {
        var result = await svc.RegisterAsync(UserId, req);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── Get profile by id ─────────────────────────────────────────────────────
    [Authorize(Roles = "DeliveryAgent,Admin")]
    [HttpGet("{id:guid}")]
     [Authorize(Roles = "Admin,DeliveryAgent,")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await svc.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // ── UC-44: Admin verify ───────────────────────────────────────────────────
   
    [HttpPut("{id:guid}/verify")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Verify(Guid id)
    {
        var result = await svc.VerifyAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // ── UC-45: Toggle availability ────────────────────────────────────────────
    [HttpPut("{id:guid}/availability")]
    [Authorize(Roles = "DeliveryAgent,Admin")]    public async Task<IActionResult> ToggleAvailability(Guid id)
    {
        var result = await svc.ToggleAvailabilityAsync(id, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── UC-47: Update GPS location ────────────────────────────────────────────
    [HttpPut("{id:guid}/location")]
    [Authorize(Roles = "DeliveryAgent,Admin")]
        public async Task<IActionResult> UpdateLocation(Guid id, [FromBody] UpdateLocationRequest req)
    {
        var result = await svc.UpdateLocationAsync(id, UserId, req);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── UC-46: View assigned orders ───────────────────────────────────────────
    [HttpGet("{id:guid}/orders")]
    [Authorize(Roles = "DeliveryAgent,Admin")]
        public async Task<IActionResult> GetAssignedOrders(Guid id)
    {
        var result = await svc.GetAssignedOrdersAsync(id, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── UC-48: Mark picked up ─────────────────────────────────────────────────
    [HttpPost("{id:guid}/pickup/{orderId:guid}")]
    [Authorize(Roles = "DeliveryAgent,Admin")]
    public async Task<IActionResult> MarkPickedUp(Guid id, Guid orderId)
    {
        var result = await svc.MarkPickedUpAsync(id, UserId, orderId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── UC-48: Mark delivered ─────────────────────────────────────────────────
    [HttpPost("{id:guid}/complete-delivery/{orderId:guid}")]
    [Authorize(Roles = "DeliveryAgent,Admin")]
    public async Task<IActionResult> MarkDelivered(Guid id, Guid orderId)
    {
        var result = await svc.MarkDeliveredAsync(id, UserId, orderId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── UC-49: Earnings & ratings ─────────────────────────────────────────────
    [HttpGet("{id:guid}/earnings")]
   [Authorize(Roles = "DeliveryAgent,Admin")]
    public async Task<IActionResult> GetEarnings(Guid id)
    {
        var result = await svc.GetEarningsAsync(id, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── UC-50: Nearby agents (System/Admin use) ───────────────────────────────
    [HttpGet("nearby")]
    [Authorize(Roles = "Admin,RestaurantOwner")]
    public async Task<IActionResult> GetNearby([FromQuery] double lat, [FromQuery] double lng, [FromQuery] double radiusKm = 5)
    {
        var result = await svc.GetNearbyAgentsAsync(lat, lng, radiusKm);
        return Ok(result);
    }

    // ── System: Assign order ──────────────────────────────────────────────────
    [HttpPost("{id:guid}/assign")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignOrder(Guid id, [FromBody] AssignOrderRequest req)
    {
        var result = await svc.AssignOrderAsync(id, req);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── System: Update rating ─────────────────────────────────────────────────
    [HttpPut("{id:guid}/rating")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateRating(Guid id, [FromBody] RateDeliveryRequest req)
    {
        var result = await svc.UpdateRatingAsync(id, req);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
