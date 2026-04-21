using System;
using System.ComponentModel.DataAnnotations;

namespace Restaurant_Service.DTOs;

public class CreateRestaurantDTO
{
    [Required]
    public string Name { get; set; } = string.Empty; 
    public string Description { get; set; } = string.Empty;   
    [Required]
    public string Cuisine { get; set; } = string.Empty;   
    [Required]
    public string Address { get; set; } = string.Empty;   
    [Required]
    [Range(-90, 90)]
    public double Latitude { get; set; }
    [Required]
    [Range(-180, 180)]
    public double Longitude { get; set; }
    public string? ImageUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public TimeSpan? OpeningTime { get; set; }
    public TimeSpan? ClosingTime { get; set; }
}
