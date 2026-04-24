using System.ComponentModel.DataAnnotations;

namespace Review_Service.DTOs;

public class AvgRatingDTO
{
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }

    public AvgRatingDTO() { }

    public AvgRatingDTO(double averageRating, int totalReviews)
    {
        AverageRating = averageRating;
        TotalReviews = totalReviews;
    }
}