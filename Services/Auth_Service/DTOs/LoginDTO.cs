using System.ComponentModel.DataAnnotations;

namespace Auth_Service.DTOs;
public class LoginDTO
{
    [Required, EmailAddress]
    public string Email { get; set; } 
        [Required]
    public string Password { get; set; } 
}