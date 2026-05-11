using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment_Service.DTOs;
using Payment_Service.Interfaces;

namespace Payment_Service.Controllers;

[ApiController]
[Route("api/v1/wallet")]
[Authorize(Roles = "Customer,Admin")]
public class WalletController(IWalletService walletService, IRazorpayService razorpayService) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet("balance")]
    public async Task<IActionResult> Balance() =>
        Ok(await walletService.GetBalanceAsync(UserId));

    [HttpPost("add")]
    public async Task<IActionResult> Add([FromBody] AddMoneyRequest req) =>
        Ok(await walletService.AddMoneyAsync(UserId, req));

    [HttpPost("topup/initiate")]
    public IActionResult InitiateTopup([FromBody] InitiateTopupRequest req)
    {
        try
        {
            if (req.Amount <= 0) return BadRequest("Amount must be greater than 0");
            
            var receiptId = $"topup_{UserId.Substring(0, 8)}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            var rzpOrderId = razorpayService.CreateOrder(req.Amount, "INR", receiptId);
            
            return Ok(new { razorpayOrderId = rzpOrderId, amount = req.Amount });
        }
        catch (Exception ex)
        {
            // Log the error (assumes a logger is available or just use Console for now)
            Console.WriteLine($"[CRITICAL] Topup Initiation Failed: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return StatusCode(500, new { message = "Error initiating Razorpay order", detail = ex.Message });
        }
    }

    [HttpPost("pay")]
    public async Task<IActionResult> Pay([FromBody] WalletPayRequest req) =>
        Ok(await walletService.PayFromWalletAsync(UserId, req));

    [HttpGet("statements")]
    public async Task<IActionResult> Statements() =>
        Ok(await walletService.GetStatementsAsync(UserId));
}
