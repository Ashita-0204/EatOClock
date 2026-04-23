using System.ComponentModel.DataAnnotations;

namespace Review_Service.DTOs;

public class SubmitReviewDTO
{
    [Required] public Guid OrderId { get; set; }
    [Required] public Guid RestaurantId { get; set; }
    public Guid? AgentId { get; set; }
    [Range(1, 5)] public int FoodRating { get; set; }
    [Range(1, 5)] public int DeliveryRating { get; set; }
    [MaxLength(1000)] public string? Comment { get; set; }
}

public class EditReviewDTO
{
    [Range(1, 5)] public int FoodRating { get; set; }
    [Range(1, 5)] public int DeliveryRating { get; set; }
    [MaxLength(1000)] public string? Comment { get; set; }
}

public class ReviewDTO
{
    public Guid ReviewId { get; set; }
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public Guid RestaurantId { get; set; }
    public Guid? AgentId { get; set; }
    public int FoodRating { get; set; }
    public int DeliveryRating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AvgRatingDTO
{
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
}
