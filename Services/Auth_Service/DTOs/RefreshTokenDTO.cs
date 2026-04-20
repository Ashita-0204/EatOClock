using System.ComponentModel.DataAnnotations;

namespace Auth_Service.DTOs;

public class RefreshTokenDTO
{
    [Required]
    public string RefreshToken { get; set; }
}