using System.ComponentModel.DataAnnotations;

namespace Menu_Service.DTOs;

public class MenuItemResponse
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsVeg { get; set; }
    public DateTime CreatedAt { get; set; }

    public MenuItemResponse() { }

    public MenuItemResponse(Guid id, Guid categoryId, Guid restaurantId, string name,
        string description, decimal price, string? imageUrl, bool isAvailable,
        bool isVeg, DateTime createdAt)
    {
        Id = id;
        CategoryId = categoryId;
        RestaurantId = restaurantId;
        Name = name;
        Description = description;
        Price = price;
        ImageUrl = imageUrl;
        IsAvailable = isAvailable;
        IsVeg = isVeg;
        CreatedAt = createdAt;
    }
}