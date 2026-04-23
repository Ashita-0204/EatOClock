using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Review_Service.DTOs;
using Review_Service.Interfaces;
using System.Security.Claims;

namespace Review_Service.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController(IReviewService svc) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    private bool IsAdmin  => User.IsInRole("Admin");

    // UC-51: POST /api/v1/reviews
    [HttpPost,Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> Submit([FromBody] SubmitReviewDTO dto)
    {
        var (ok, error, data) = await svc.SubmitReviewAsync(dto, UserId);
        return ok ? Ok(data) : BadRequest(new { error });
    }

    // UC-53: GET /api/v1/reviews/restaurant/{rId}
    [HttpGet("restaurant/{rId:guid}"),Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> RestaurantReviews(Guid rId) =>
        Ok(await svc.GetRestaurantReviewsAsync(rId));

    // UC-54: GET /api/v1/reviews/agent/{aId}
    [HttpGet("agent/{aId:guid}"),Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> AgentReviews(Guid aId) =>
        Ok(await svc.GetAgentReviewsAsync(aId));

    // GET /api/v1/reviews/order/{oId}
    [HttpGet("order/{oId:guid}"),Authorize(Roles = "Admin,DeliveryAgent,RestaurantOwner")]
    public async Task<IActionResult> OrderReview(Guid oId)
    {
        var review = await svc.GetOrderReviewAsync(oId, UserId, IsAdmin);
        return review == null ? NotFound() : Ok(review);
    }

    // PUT /api/v1/reviews/{id}
    [HttpPut("{id:guid}"),Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> Edit(Guid id, [FromBody] EditReviewDTO dto)
    {
        var (ok, error) = await svc.EditReviewAsync(id, dto, UserId);
        return ok ? NoContent() : BadRequest(new { error });
    }

    // UC-55: DELETE /api/v1/reviews/{id}  [Admin]
    [HttpDelete("{id:guid}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Moderate(Guid id)
    {
        var (ok, error) = await svc.DeleteReviewAsync(id);
        return ok ? NoContent() : NotFound(new { error });
    }

    // UC-56: GET /api/v1/reviews/avg/restaurant/{rId}
    [HttpGet("avg/restaurant/{rId:guid}"),Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> AvgRestaurant(Guid rId) =>
        Ok(await svc.GetAvgRestaurantRatingAsync(rId));

    // UC-56: GET /api/v1/reviews/avg/agent/{aId}
    [HttpGet("avg/agent/{aId:guid}"),Authorize(Roles = "Admin,Customer,RestaurantOwner")]
    public async Task<IActionResult> AvgAgent(Guid aId) =>
        Ok(await svc.GetAvgAgentRatingAsync(aId));
}
