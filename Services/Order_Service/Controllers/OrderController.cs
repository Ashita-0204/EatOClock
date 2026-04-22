using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order_Service.DTOs;
using Order_Service.Interfaces;

namespace Order_Service.Controllers;

[ApiController]
[Route("api/v1/orders")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderService _svc;
    public OrderController(IOrderService svc) => _svc = svc;

    private string CallerId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("User id claim missing.");

    private string CallerRole =>
        User.FindFirstValue(ClaimTypes.Role) ?? "Customer";

    // UC-29: Place order
    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest req)
    {
        try
        {
            var order = await _svc.PlaceOrderAsync(CallerId, req);
            return CreatedAtAction(nameof(GetById), new { id = order.OrderId },
                new ApiResponse<OrderDto>(true, "Order placed.", order));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object>(false, ex.Message, null));
        }
    }

    // UC-30: Get order by ID
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var order = await _svc.GetByIdAsync(id, CallerId, CallerRole);
            if (order == null) return NotFound(new ApiResponse<object>(false, "Not found.", null));
            return Ok(new ApiResponse<OrderDto>(true, null, order));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<object>(false, ex.Message, null));
        }
    }

    // UC-32: Customer order history
    [HttpGet("customer")]
    public async Task<IActionResult> GetMyOrders()
    {
        var orders = await _svc.GetCustomerOrdersAsync(CallerId);
        return Ok(new ApiResponse<List<OrderDto>>(true, null, orders));
    }

    // UC-34: Restaurant orders
    [HttpGet("restaurant/{rId:guid}")]
    [Authorize(Roles = "RestaurantOwner,Admin")]
    public async Task<IActionResult> GetRestaurantOrders(Guid rId)
    {
        var orders = await _svc.GetRestaurantOrdersAsync(rId);
        return Ok(new ApiResponse<List<OrderDto>>(true, null, orders));
    }

    // UC-30/34: Update status
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "RestaurantOwner,DeliveryAgent,Admin")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest req)
    {
        try
        {
            var order = await _svc.UpdateStatusAsync(id, req, CallerId, CallerRole);
            return Ok(new ApiResponse<OrderDto>(true, "Status updated.", order));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object>(false, ex.Message, null));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<object>(false, ex.Message, null));
        }
    }

    // UC-31: Cancel order
    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderRequest req)
    {
        try
        {
            var order = await _svc.CancelOrderAsync(id, CallerId);
            return Ok(new ApiResponse<OrderDto>(true, "Order cancelled.", order));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object>(false, ex.Message, null));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<object>(false, ex.Message, null));
        }
    }

    // UC-33: Reorder
    [HttpPost("{id:guid}/reorder")]
    public async Task<IActionResult> Reorder(Guid id)
    {
        try
        {
            var order = await _svc.ReorderAsync(id, CallerId);
            return CreatedAtAction(nameof(GetById), new { id = order.OrderId },
                new ApiResponse<OrderDto>(true, "Reorder placed.", order));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<object>(false, ex.Message, null));
        }
    }

    // UC-36: Assign delivery agent (internal/system)
    [HttpPut("{id:guid}/assign-agent")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignAgent(Guid id, [FromBody] AssignAgentRequest req)
    {
        try
        {
            var order = await _svc.AssignAgentAsync(id, req);
            return Ok(new ApiResponse<OrderDto>(true, "Agent assigned.", order));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<object>(false, ex.Message, null));
        }
    }

    // UC-35: Admin — all orders
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _svc.GetAllOrdersAsync();
        return Ok(new ApiResponse<List<OrderDto>>(true, null, orders));
    }
}
