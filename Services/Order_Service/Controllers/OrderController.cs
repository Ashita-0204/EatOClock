using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order_Service.DTOs;
using Order_Service.Interfaces;

namespace Order_Service.Controllers;

[ApiController]
[Route("api/v1/orders")]

public class OrderController : ControllerBase
{
    private readonly IOrderService _svc;
    private readonly IHttpClientFactory _httpClientFactory;
    public OrderController(IOrderService svc, IHttpClientFactory httpClientFactory)
    {
        _svc = svc;
        _httpClientFactory = httpClientFactory;
    }

    private string CallerId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException("User id claim missing.");

    private string CallerRole =>
        User.FindFirstValue(ClaimTypes.Role) ?? "Customer";

    private string CallerEmail =>
        User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

    //Place order
    [Authorize(Roles = "Customer,Admin")]
    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest req)
    {
        Console.WriteLine($"[Order_Service] Placing order for customer: {CallerId} at restaurant: {req.RestaurantId}");
        try
        {
            var order = await _svc.PlaceOrderAsync(CallerId, req);
            Console.WriteLine($"[Order_Service] Order created: {order.OrderId}");
            
            // Send email notification via Notification_Service
            try
            {
                var client = _httpClientFactory.CreateClient("NotificationService");
                var notificationDto = new
                {
                    RecipientId = CallerId,
                    OrderId = order.OrderId,
                    OrderStatus = order.Status,
                    Email = CallerEmail
                };
                Console.WriteLine($"[Order_Service] Sending notification to {CallerEmail}...");
                await client.PostAsJsonAsync("/api/notifications/order", notificationDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Order_Service] Failed to send notification: {ex.Message}");
            }

            return CreatedAtAction(nameof(GetById), new { id = order.OrderId },
                new ApiResponse<OrderDTOs>(true, "Order placed.", order));
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"[Order_Service] Validation error: {ex.Message}");
            return BadRequest(new ApiResponse<object>(false, ex.Message, null));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Order_Service] UNEXPECTED ERROR: {ex.Message}\n{ex.StackTrace}");
            return StatusCode(500, new ApiResponse<object>(false, "An unexpected error occurred while placing the order.", null));
        }
    }

    //Get order by ID
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var order = await _svc.GetByIdAsync(id, CallerId, CallerRole);
            if (order == null) return NotFound(new ApiResponse<object>(false, "Not found.", null));
            return Ok(new ApiResponse<OrderDTOs>(true, null, order));
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

    //  Customer order history
    [Authorize(Roles = "Customer,Admin")]
    [HttpGet("customer")]
    public async Task<IActionResult> GetMyOrders()
    {
        var orders = await _svc.GetCustomerOrdersAsync(CallerId);
        return Ok(new ApiResponse<List<OrderDTOs>>(true, null, orders));
    }

    //  Restaurant orders
    [HttpGet("restaurant/{rId:guid}")]
    [Authorize(Roles = "RestaurantOwner,Admin")]
    public async Task<IActionResult> GetRestaurantOrders(Guid rId)
    {
        var orders = await _svc.GetRestaurantOrdersAsync(rId);
        return Ok(new ApiResponse<List<OrderDTOs>>(true, null, orders));
    }

    //  Update status
    [Authorize(Roles = "RestaurantOwner,Admin,DeliveryAgent")]
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest req)
    {
        try
        {
            var order = await _svc.UpdateStatusAsync(id, req, CallerId, CallerRole);
            return Ok(new ApiResponse<OrderDTOs>(true, "Status updated.", order));
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

    //  Cancel order
    [HttpPut("{id:guid}/cancel")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderRequest req)
    {
        try
        {
            var order = await _svc.CancelOrderAsync(id, CallerId);
            return Ok(new ApiResponse<OrderDTOs>(true, "Order cancelled.", order));
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

    //  Reorder
    [HttpPost("{id:guid}/reorder")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<IActionResult> Reorder(Guid id)
    {
        try
        {
            var order = await _svc.ReorderAsync(id, CallerId);
            return CreatedAtAction(nameof(GetById), new { id = order.OrderId },
                new ApiResponse<OrderDTOs>(true, "Reorder placed.", order));
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

    //  Confirm Order
    [HttpPut("{id:guid}/confirm")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<IActionResult> ConfirmOrder(Guid id)
    {
        try
        {
            // First check if it's their order
            var orderCheck = await _svc.GetByIdAsync(id, CallerId, CallerRole);
            if (orderCheck == null) return NotFound();

            var order = await _svc.ConfirmOrderAsync(id);
            return Ok(new ApiResponse<OrderDTOs>(true, "Order confirmed.", order));
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

    //  Assign delivery agent (internal/system)
    [HttpPut("{id:guid}/assign-agent")]
    [Authorize(Roles = "Admin,RestaurantOwner,DeliveryAgent")]
    public async Task<IActionResult> AssignAgent(Guid id, [FromBody] AssignAgentRequest req)
    {
        try
        {
            var order = await _svc.AssignAgentAsync(id, req);
            return Ok(new ApiResponse<OrderDTOs>(true, "Agent assigned.", order));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<object>(false, ex.Message, null));
        }
    }

    //  Admin - all orders
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _svc.GetAllOrdersAsync();
        return Ok(new ApiResponse<List<OrderDTOs>>(true, null, orders));
    }

    //  Agent - available orders
    [HttpGet("available")]
    [Authorize(Roles = "Admin,DeliveryAgent")]
    public async Task<IActionResult> GetAvailableOrders()
    {
        var orders = await _svc.GetAvailableOrdersAsync();
        return Ok(new ApiResponse<List<OrderDTOs>>(true, null, orders));
    }

    [HttpGet("agent/{aId}")]
    [Authorize(Roles = "Admin,DeliveryAgent")]
    public async Task<IActionResult> GetAgentOrders(string aId)
    {
        var orders = await _svc.GetAgentOrdersAsync(aId);
        return Ok(new ApiResponse<List<OrderDTOs>>(true, null, orders));
    }
}
