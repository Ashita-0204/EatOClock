using Microsoft.AspNetCore.Identity;
using System;
namespace Auth_Service.Models;

public class User : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? CreatedAt { get; set; }
}