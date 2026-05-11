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
using Auth_Service.Models;

namespace Auth_Service.Interfaces;
public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterDTO dto);
    Task<AuthResult> LoginAsync(LoginDTO dto);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
    Task<UserDTO> GetProfileAsync(string userId);
    Task<UserDTO> GetUserByIdAsync(string userId);
    Task<bool> UpdateProfileAsync(string userId, string fullName, string? phoneNumber);
    Task<bool> ChangePasswordAsync(string userId, ChangePasswordDTO dto);
    Task<bool> DeactivateAccountAsync(string userId);
    Task<bool> AssignAdminAsync(string userId);
    Task<string> BootstrapAdminAsync(string userId);
}