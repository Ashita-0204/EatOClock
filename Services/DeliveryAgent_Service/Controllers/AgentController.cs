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

    //  Register -------------------------------------------------------
    [HttpPost("register")]
    [Authorize(Roles = "DeliveryAgent,Admin")]
    public async Task<IActionResult> Register([FromBody] RegisterAgentRequest req)
    {
        var result = await svc.RegisterAsync(UserId, req);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // -- Get profile by id -----------------------------------------------------
    [Authorize(Roles = "DeliveryAgent,Admin")]
    [HttpGet("{id:guid}")]
     [Authorize(Roles = "Admin,DeliveryAgent,")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await svc.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // -- Get my profile --------------------------------------------------------
    [Authorize(Roles = "DeliveryAgent,Admin")]
    [HttpGet("my-profile")]
    public async Task<IActionResult> GetMyProfile()
    {
        var result = await svc.GetByUserIdAsync(UserId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    //  Admin verify ---------------------------------------------------
   
    [HttpPut("{id:guid}/verify")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Verify(Guid id)
    {
        var result = await svc.VerifyAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id:guid}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(Guid id)
    {
        var result = await svc.RejectAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    //  Toggle availability --------------------------------------------
    [HttpPut("{id:guid}/availability")]
    [Authorize(Roles = "DeliveryAgent,Admin")]   
     public async Task<IActionResult> ToggleAvailability(Guid id)
    {
        var result = await svc.ToggleAvailabilityAsync(id, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    //  Update GPS location --------------------------------------------
    [HttpPut("{id:guid}/location")]
    [Authorize(Roles = "DeliveryAgent,Admin")]
        public async Task<IActionResult> UpdateLocation(Guid id, [FromBody] UpdateLocationRequest req)
    {
        var result = await svc.UpdateLocationAsync(id, UserId, req);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // View assigned orders -------------------------------------------
    [HttpGet("{id:guid}/orders")]
    [Authorize(Roles = "DeliveryAgent,Admin")]
        public async Task<IActionResult> GetAssignedOrders(Guid id)
    {
        var result = await svc.GetAssignedOrdersAsync(id, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    //  Mark picked up -------------------------------------------------
    [HttpPost("{id:guid}/pickup/{orderId:guid}")]
    [Authorize(Roles = "DeliveryAgent,Admin")]
    public async Task<IActionResult> MarkPickedUp(Guid id, Guid orderId)
    {
        var result = await svc.MarkPickedUpAsync(id, UserId, orderId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Mark delivered -------------------------------------------------
    [HttpPost("{id:guid}/complete-delivery/{orderId:guid}")]
    [Authorize(Roles = "DeliveryAgent,Admin")]
    public async Task<IActionResult> MarkDelivered(Guid id, Guid orderId)
    {
        var result = await svc.MarkDeliveredAsync(id, UserId, orderId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    //Earnings & ratings ---------------------------------------------
    [HttpGet("{id:guid}/earnings")]
   [Authorize(Roles = "DeliveryAgent,Admin")]
    public async Task<IActionResult> GetEarnings(Guid id)
    {
        var result = await svc.GetEarningsAsync(id, UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Nearby agents (System/Admin use) -------------------------------
    [HttpGet("nearby")]
    [Authorize(Roles = "Admin,RestaurantOwner")]
    public async Task<IActionResult> GetNearby([FromQuery] double lat, [FromQuery] double lng, [FromQuery] double radiusKm = 5)
    {
        var result = await svc.GetNearbyAgentsAsync(lat, lng, radiusKm);
        return Ok(result);
    }

    // Get all agents (Admin use) --------------------------------------
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllAgents()
    {
        var result = await svc.GetAllAgentsAsync();
        return Ok(result);
    }

    // -- System: Assign order --------------------------------------------------
    [HttpPost("{id:guid}/assign")]
    [Authorize(Roles = "DeliveryAgent,Admin")]
    public async Task<IActionResult> AssignOrder(Guid id, [FromBody] AssignOrderRequest req)
    {
        var result = await svc.AssignOrderAsync(id, req);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // -- System: Update rating -------------------------------------------------
    [HttpPut("{id:guid}/rating")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateRating(Guid id, [FromBody] RateDeliveryRequest req)
    {
        var result = await svc.UpdateRatingAsync(id, req);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
