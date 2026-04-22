using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment_Service.DTOs;
using Payment_Service.Interfaces;

namespace Payment_Service.Controllers;

[ApiController]
[Route("api/v1/wallet")]
[Authorize]
public class WalletController(IWalletService walletService) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet("balance")]
    public async Task<IActionResult> Balance() =>
        Ok(await walletService.GetBalanceAsync(UserId));

    [HttpPost("add")]
    public async Task<IActionResult> Add([FromBody] AddMoneyRequest req) =>
        Ok(await walletService.AddMoneyAsync(UserId, req));

    [HttpPost("pay")]
    public async Task<IActionResult> Pay([FromBody] WalletPayRequest req) =>
        Ok(await walletService.PayFromWalletAsync(UserId, req));

    [HttpGet("statements")]
    public async Task<IActionResult> Statements() =>
        Ok(await walletService.GetStatementsAsync(UserId));
}
