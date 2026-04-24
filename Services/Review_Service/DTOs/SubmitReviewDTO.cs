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

    public SubmitReviewDTO() { }

    public SubmitReviewDTO(Guid orderId, Guid restaurantId, Guid? agentId,
        int foodRating, int deliveryRating, string? comment)
    {
        OrderId = orderId;
        RestaurantId = restaurantId;
        AgentId = agentId;
        FoodRating = foodRating;
        DeliveryRating = deliveryRating;
        Comment = comment;
    }
}
