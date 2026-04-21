using System;
using System.ComponentModel.DataAnnotations;

namespace Restaurant_Service.DTOs;
public class NearbySearchDTO
{
    [Required]
    [Range(-90, 90)]
    public double Latitude { get; set; }
    [Required]
    [Range(-180, 180)]
    public double Longitude { get; set; }
    [Range(0.1, 100)]
    public double RadiusKm { get; set; } = 5.0;
    public string? Cuisine { get; set; }
    public double? MinRating { get; set; }
}
