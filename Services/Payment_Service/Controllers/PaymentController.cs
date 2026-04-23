using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment_Service.DTOs;
using Payment_Service.Interfaces;

namespace Payment_Service.Controllers;

[ApiController]
[Route("api/v1/payments")]
public class PaymentController(IPaymentService paymentService) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private bool IsAdmin => User.IsInRole("Admin");


    [Authorize(Roles = "Customer,Admin")]
    [HttpPost("process")]
    public async Task<IActionResult> Process([FromBody] ProcessPaymentRequest req)
    {
        var (payment, rzpOrderId) = await paymentService.ProcessPaymentAsync(UserId, req);
        return rzpOrderId != null
            ? Ok(new { payment, razorpayOrderId = rzpOrderId, message = "Complete payment using Razorpay" })
            : Ok(payment);
    }

    [HttpPost("refund")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<IActionResult> Refund([FromBody] RefundRequest req)
    {
        var result = await paymentService.RefundPaymentAsync(UserId, IsAdmin, req);
        return Ok(result);
    }

    [HttpGet("order/{orderId:guid}")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<IActionResult> GetByOrder(Guid orderId)
    {
        var result = await paymentService.GetByOrderIdAsync(UserId, orderId);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("customer")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<IActionResult> GetHistory() =>
        Ok(await paymentService.GetCustomerHistoryAsync(UserId));

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll() =>
        Ok(await paymentService.GetAllTransactionsAsync());
}
