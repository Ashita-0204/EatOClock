using System.ComponentModel.DataAnnotations;

namespace Review_Service.DTOs;
public class EditReviewDTO
{
    [Range(1, 5)] public int FoodRating { get; set; }
    [Range(1, 5)] public int DeliveryRating { get; set; }
    [MaxLength(1000)] public string? Comment { get; set; }

    public EditReviewDTO() { }

    public EditReviewDTO(int foodRating, int deliveryRating, string? comment)
    {
        FoodRating = foodRating;
        DeliveryRating = deliveryRating;
        Comment = comment;
    }
}
