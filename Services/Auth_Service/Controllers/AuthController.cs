using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Auth_Service.DTOs;
using Auth_Service.Services;
using Auth_Service.Interfaces;

namespace Auth_Service.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
    {
        var result = await _authService.RegisterAsync(dto);
        if (!result.Success)
            return BadRequest(new { message = result.Message });
        
        return Ok(new { 
            message = "Registration successful", 
            accessToken = result.AccessToken,
            refreshToken = result.RefreshToken,
            user = result.User 
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        var result = await _authService.LoginAsync(dto);
        if (!result.Success)
            return Unauthorized(new { message = result.Message });
        
        return Ok(new { 
            message = "Login successful", 
            accessToken = result.AccessToken,
            refreshToken = result.RefreshToken,
            user = result.User 
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDTO dto)
    {
        var result = await _authService.RefreshTokenAsync(dto.RefreshToken);
        if (!result.Success)
            return Unauthorized(new { message = result.Message });
        
        return Ok(new { 
            accessToken = result.AccessToken,
            refreshToken = result.RefreshToken,
            user = result.User 
        });
    }

    [HttpPost("bootstrap-admin/{userId}")]
public async Task<IActionResult> BootstrapAdmin(string userId)
{
    var result = await _authService.BootstrapAdminAsync(userId);
    if (result == "no_admins_exist")
        return Ok(new { message = "Admin role assigned successfully" });
    if (result == "admins_already_exist")
        return BadRequest(new { message = "Bootstrap not allowed: an admin already exists. Use assign-admin instead." });
    return NotFound(new { message = "User not found" });
}

     [Authorize(Roles = "Admin")]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var profile = await _authService.GetProfileAsync(userId);
        return Ok(profile);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserById(string userId)
    {
        var user = await _authService.GetUserByIdAsync(userId);
        if (user == null) return NotFound();
        return Ok(user);
    }

     [Authorize(Roles = "Admin")]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var result = await _authService.UpdateProfileAsync(userId, request.FullName, request.PhoneNumber);
        return result ? Ok(new { message = "Profile updated successfully" }) : BadRequest(new { message = "Update failed" });
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var result = await _authService.ChangePasswordAsync(userId, dto);
        return result ? Ok(new { message = "Password changed successfully" }) : BadRequest(new { message = "Current password is incorrect" });
    }

    [HttpDelete("deactivate")]
    public async Task<IActionResult> DeactivateAccount()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var result = await _authService.DeactivateAccountAsync(userId);
        return result ? Ok(new { message = "Account deactivated" }) : BadRequest(new { message = "Deactivation failed" });
    }
  [Authorize(Roles = "Admin")]
[HttpPost("assign-admin/{userId}")]
public async Task<IActionResult> AssignAdmin(string userId)
{
    var result = await _authService.AssignAdminAsync(userId);
    return result ? Ok(new { message = "Admin role assigned" }) : NotFound(new { message = "User not found" });
}
}