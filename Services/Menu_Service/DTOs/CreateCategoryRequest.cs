using System.ComponentModel.DataAnnotations;

namespace Menu_Service.DTOs;

// -- Category ----------------------------------------------

public class CreateCategoryRequest
{
    [Required]
    public Guid RestaurantId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = "";

    public int DisplayOrder { get; set; } = 0;
    public CreateCategoryRequest() { }

    public CreateCategoryRequest(Guid restaurantId, string name, string description, int displayOrder)
    {
        RestaurantId = restaurantId;
        Name = name;
        Description = description;
        DisplayOrder = displayOrder;
    }
}
