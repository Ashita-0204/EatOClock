using System.ComponentModel.DataAnnotations;
using Auth_Service.Enums;
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
    public AllowedRegistrationRole Role { get; set; }
     [RegularExpression(@"^\+?[0-9]{10}$", ErrorMessage = "Phone number must be 10 digits ")]
        public string? PhoneNumber { get; set; }
}