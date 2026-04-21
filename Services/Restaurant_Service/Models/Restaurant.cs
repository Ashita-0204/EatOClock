using System;
using System.ComponentModel.DataAnnotations;

namespace Restaurant_Service.Models;

public class Restaurant
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();  
    [Required]
    public string Name { get; set; } = string.Empty;  
    public string Description { get; set; } = string.Empty;   
    [Required]
    public string Cuisine { get; set; } = string.Empty;   
    [Required]
    public string Address { get; set; } = string.Empty;   
    [Required]
    public double Latitude { get; set; }   
    [Required]
    public double Longitude { get; set; }  
    public string? ImageUrl { get; set; }  
    [Range(0, 5)]
    public double Rating { get; set; } = 0.0;  
    public int ReviewCount { get; set; } = 0;  
    [Required]
    public string OwnerId { get; set; } = string.Empty;  
    public bool IsApproved { get; set; } = false;  
    public bool IsActive { get; set; } = true;  
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  
    public DateTime? UpdatedAt { get; set; }  
    public string? PhoneNumber { get; set; }  
    public string? Email { get; set; }  
    public TimeSpan? OpeningTime { get; set; }  
    public TimeSpan? ClosingTime { get; set; }
}
