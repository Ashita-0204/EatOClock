using System.ComponentModel.DataAnnotations;
using Auth_Service.Enums;

namespace Auth_Service.DTOs;

public class RegisterDTO
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// 1 = Customer, 2 = RestaurantOwner, 3 = DeliveryAgent
    /// Admin (0 or anything else) is rejected.
    /// </summary>
    [Required]
    [Range(1, 3, ErrorMessage = "Role must be 1 (Customer), 2 (RestaurantOwner), or 3 (DeliveryAgent).")]
    public int Role { get; set; }

    [RegularExpression(@"^\+?[0-9]{10}$", ErrorMessage = "Phone number must be 10 digits")]
    public string? PhoneNumber { get; set; }

    /// <summary>Converts the integer role to the AllowedRegistrationRole enum.</summary>
    public AllowedRegistrationRole GetRole() => (AllowedRegistrationRole)Role;
}