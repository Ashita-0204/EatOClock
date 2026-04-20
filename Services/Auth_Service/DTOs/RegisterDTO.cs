using System.ComponentModel.DataAnnotations;

namespace Auth_Service.DTOs;
public class RegisterDTO
{
    [Required, EmailAddress]
    public string Email { get; set; } 
    [Required, MinLength(6)]
    public string Password { get; set; } 
    [Required]
    public string FullName { get; set; }
       [Required]
    public string Role { get; set; }   
    public string? PhoneNumber { get; set; }
}