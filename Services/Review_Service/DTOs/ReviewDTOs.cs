using System.ComponentModel.DataAnnotations;

namespace Review_Service.DTOs;

public class ReviewDTOs
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

    public ReviewDTOs() { }

    public ReviewDTOs(Guid reviewId, Guid orderId, string customerId,
        Guid restaurantId, Guid? agentId, int foodRating,
        int deliveryRating, string? comment, DateTime createdAt)
    {
        ReviewId = reviewId;
        OrderId = orderId;
        CustomerId = customerId;
        RestaurantId = restaurantId;
        AgentId = agentId;
        FoodRating = foodRating;
        DeliveryRating = deliveryRating;
        Comment = comment;
        CreatedAt = createdAt;
    }
}
