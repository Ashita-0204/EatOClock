using Review_Service.DTOs;

namespace Review_Service.Interfaces;

public interface IReviewService
{
    Task<(bool ok, string error, ReviewDTO? data)> SubmitReviewAsync(SubmitReviewDTO dto, string customerId);
    Task<List<ReviewDTO>> GetRestaurantReviewsAsync(Guid restaurantId);
    Task<List<ReviewDTO>> GetAgentReviewsAsync(Guid agentId);
    Task<ReviewDTO?> GetOrderReviewAsync(Guid orderId, string customerId, bool isAdmin);
    Task<(bool ok, string error)> EditReviewAsync(Guid reviewId, EditReviewDTO dto, string customerId);
    Task<(bool ok, string error)> DeleteReviewAsync(Guid reviewId);   // Admin moderate
    Task<AvgRatingDTO> GetAvgRestaurantRatingAsync(Guid restaurantId);
    Task<AvgRatingDTO> GetAvgAgentRatingAsync(Guid agentId);
}
