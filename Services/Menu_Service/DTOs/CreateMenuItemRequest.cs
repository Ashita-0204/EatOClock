using System.ComponentModel.DataAnnotations;
namespace Menu_Service.DTOs;
// -- Menu Item ---------------------------------------------

public class CreateMenuItemRequest
{
    [Required]
    public Guid CategoryId { get; set; }

    [Required]
    public Guid RestaurantId { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = "";

    [Required, Range(0.01, double.MaxValue)]
    public decimal Price { get; set; } = 0;

    public string? ImageUrl { get; set; } = null;

    public bool IsVeg { get; set; } = false;
 public CreateMenuItemRequest() { }

    public CreateMenuItemRequest(Guid categoryId, Guid restaurantId, string name,
        string description, decimal price, string? imageUrl, bool isVeg)
    {
        CategoryId = categoryId;
        RestaurantId = restaurantId;
        Name = name;
        Description = description;
        Price = price;
        ImageUrl = imageUrl;
        IsVeg = isVeg;
    }
    }
