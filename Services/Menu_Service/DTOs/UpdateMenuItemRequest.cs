using System.ComponentModel.DataAnnotations;

namespace Menu_Service.DTOs;

public class UpdateMenuItemRequest
{
    [MaxLength(150)]
    public string? Name { get; set; }

    public string? Description { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal? Price { get; set; }

    public string? ImageUrl { get; set; }

    public bool? IsVeg { get; set; }

    public Guid? CategoryId { get; set; }
     public UpdateMenuItemRequest() { }

    public UpdateMenuItemRequest(string? name, string? description, decimal? price,
        string? imageUrl, bool? isVeg, Guid? categoryId)
    {
        Name = name;
        Description = description;
        Price = price;
        ImageUrl = imageUrl;
        IsVeg = isVeg;
        CategoryId = categoryId;
    }
}
