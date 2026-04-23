using System.ComponentModel.DataAnnotations;

namespace Review_Service.Models;

public class Review
{
    public Guid ReviewId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid OrderId { get; set; }       // UNIQUE constraint in DB

    [Required]
    public string CustomerId { get; set; } = string.Empty;

    [Required]
    public Guid RestaurantId { get; set; }

    public Guid? AgentId { get; set; }

    [Range(1, 5)]
    public int FoodRating { get; set; }

    [Range(1, 5)]
    public int DeliveryRating { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }

    public bool IsActive { get; set; } = true;   // false = moderated/removed

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
