using System;
using System.ComponentModel.DataAnnotations;

namespace Restaurant_Service.DTOs;
public class UpdateRestaurantDTO
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Cuisine { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? ImageUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public TimeSpan? OpeningTime { get; set; }
    public TimeSpan? ClosingTime { get; set; }
}


