using System.ComponentModel.DataAnnotations;

namespace Menu_Service.DTOs;

// -- Category ----------------------------------------------

public class CategoryResponse
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<MenuItemResponse> Items { get; set; } = new();
 public CategoryResponse() { }

    public CategoryResponse(Guid id, Guid restaurantId, string name, string description,
        int displayOrder, bool isActive, DateTime createdAt, List<MenuItemResponse> items)
    {
        Id = id;
        RestaurantId = restaurantId;
        Name = name;
        Description = description;
        DisplayOrder = displayOrder;
        IsActive = isActive;
        CreatedAt = createdAt;
        Items = items;
    }
    }
