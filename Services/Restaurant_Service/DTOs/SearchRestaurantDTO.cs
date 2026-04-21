using System;
using System.ComponentModel.DataAnnotations;

namespace Restaurant_Service.DTOs;
public class SearchRestaurantDTO
{
    public string? Cuisine { get; set; }
    public double? MinRating { get; set; }
    public bool? IsApproved { get; set; }
    public string? SearchTerm { get; set; }
}

