using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Auth_Service.Data;
using Auth_Service.DTOs;
using System.Collections.Generic;
using Auth_Service.Interfaces; 
using Auth_Service.Models;

namespace Auth_Service.Services;
public class AuthServiceImpl : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _config;
    private readonly AppDbContext _context;

    public AuthServiceImpl(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, 
                       IConfiguration config, AppDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _config = config;
        _context = context;
    }

   public async Task<AuthResult> RegisterAsync(RegisterDTO dto)
{
    if (await _userManager.FindByEmailAsync(dto.Email) != null)
        return new AuthResult { Success = false, Message = "Email already registered" };

     // "Customer", "RestaurantOwner", or "DeliveryAgent"
var roleName = dto.Role.ToString(); // "Customer", "RestaurantOwner", or "DeliveryAgent"

    // Ensure role exists (seeded, but safety check)
    if (!await _roleManager.RoleExistsAsync(roleName))
        return new AuthResult { Success = false, Message = "Invalid role selected" };

    var user = new User
    {
        UserName = dto.Email,
        Email = dto.Email,
        FullName = dto.FullName,
        PhoneNumber = dto.PhoneNumber,
        CreatedAt = DateTime.UtcNow
    };

    var result = await _userManager.CreateAsync(user, dto.Password);
    if (!result.Succeeded)
        return new AuthResult { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) };

    await _userManager.AddToRoleAsync(user, roleName);

    var accessToken = await GenerateJwtToken(user);
    var refreshToken = GenerateRefreshToken();

    user.RefreshToken = refreshToken;
    user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
    await _userManager.UpdateAsync(user);

    return new AuthResult
    {
        Success = true,
        AccessToken = accessToken,
        RefreshToken = refreshToken,
        User = new UserDTO
        {
            Id = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Role = roleName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive
        }
    };
}
    public async Task<AuthResult> LoginAsync(LoginDTO dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return new AuthResult { Success = false, Message = "Invalid email or password" };

        if (!user.IsActive)
            return new AuthResult { Success = false, Message = "Account is deactivated" };

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = await GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return new AuthResult
        {
            Success = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = new UserDTO
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                Role = roles.FirstOrDefault() ?? "Customer",
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive
            }
        };
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        
        if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
            return new AuthResult { Success = false, Message = "Invalid or expired refresh token" };

        var newAccessToken = await GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);

        return new AuthResult
        {
            Success = true,
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            User = new UserDTO
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                Role = roles.FirstOrDefault() ?? "Customer",
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive
            }
        };
    }

    public async Task<UserDTO> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) throw new Exception("User not found");

        var roles = await _userManager.GetRolesAsync(user);
        
        return new UserDTO
        {
            Id = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Role = roles.FirstOrDefault() ?? "Customer",
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive
        };
    }

    public async Task<bool> UpdateProfileAsync(string userId, string fullName, string? phoneNumber)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        user.FullName = fullName;
        if (phoneNumber != null) user.PhoneNumber = phoneNumber;
        
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDTO dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        return result.Succeeded;
    }

    public async Task<bool> DeactivateAccountAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        user.IsActive = false;
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    private async Task<string> GenerateJwtToken(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim("userId", user.Id)
        };
        
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(48),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public async Task<string> BootstrapAdminAsync(string userId)
{
    // Lock the endpoint once any admin exists
    var adminsExist = await _userManager.GetUsersInRoleAsync("Admin");
    if (adminsExist.Count > 0)
        return "admins_already_exist";

    var user = await _userManager.FindByIdAsync(userId);
    if (user == null) return "user_not_found";

    await _userManager.AddToRoleAsync(user, "Admin");
    return "no_admins_exist";
}
public async Task<bool> AssignAdminAsync(string userId)
{
    var user = await _userManager.FindByIdAsync(userId);
    if (user == null) return false;

    if (!await _userManager.IsInRoleAsync(user, "Admin"))
        await _userManager.AddToRoleAsync(user, "Admin");

    return true;
}

}