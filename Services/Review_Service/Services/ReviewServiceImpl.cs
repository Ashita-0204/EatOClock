using Microsoft.EntityFrameworkCore;
using Review_Service.Data;
using Review_Service.DTOs;
using Review_Service.Interfaces;
using Review_Service.Models;

namespace Review_Service.Services;

public class ReviewServiceImpl(AppDbContext db) : IReviewService
{
    // UC-51 + UC-52
    public async Task<(bool ok, string error, ReviewDTOs? data)> SubmitReviewAsync(SubmitReviewDTO dto, string customerId)
    {
        if (await db.Reviews.AnyAsync(r => r.OrderId == dto.OrderId))
            return (false, "Review already exists for this order.", null);

        var review = new Review
        {
            OrderId      = dto.OrderId,
            CustomerId   = customerId,
            RestaurantId = dto.RestaurantId,
            AgentId      = dto.AgentId,
            FoodRating   = dto.FoodRating,
            DeliveryRating = dto.DeliveryRating,
            Comment      = dto.Comment
        };

        db.Reviews.Add(review);
        await db.SaveChangesAsync();
        return (true, string.Empty, Map(review));
    }

    // UC-53
    public async Task<List<ReviewDTOs>> GetRestaurantReviewsAsync(Guid restaurantId) =>
        await db.Reviews
            .Where(r => r.RestaurantId == restaurantId && r.IsActive)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => Map(r))
            .ToListAsync();

    // UC-54
    public async Task<List<ReviewDTOs>> GetAgentReviewsAsync(Guid agentId) =>
        await db.Reviews
            .Where(r => r.AgentId == agentId && r.IsActive)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => Map(r))
            .ToListAsync();

    public async Task<ReviewDTOs?> GetOrderReviewAsync(Guid orderId, string customerId, bool isAdmin)
    {
        var review = await db.Reviews.FirstOrDefaultAsync(r => r.OrderId == orderId && r.IsActive);
        if (review == null) return null;
        if (!isAdmin && review.CustomerId != customerId) return null;
        return Map(review);
    }

    public async Task<(bool ok, string error)> EditReviewAsync(Guid reviewId, EditReviewDTO dto, string customerId)
    {
        var review = await db.Reviews.FindAsync(reviewId);
        if (review == null || !review.IsActive) return (false, "Review not found.");
        if (review.CustomerId != customerId) return (false, "Forbidden.");

        review.FoodRating     = dto.FoodRating;
        review.DeliveryRating = dto.DeliveryRating;
        review.Comment        = dto.Comment;
        review.UpdatedAt      = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return (true, string.Empty);
    }

    // UC-55
    public async Task<(bool ok, string error)> DeleteReviewAsync(Guid reviewId)
    {
        var review = await db.Reviews.FindAsync(reviewId);
        if (review == null) return (false, "Review not found.");

        review.IsActive  = false;
        review.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return (true, string.Empty);
    }

    // UC-56
    public async Task<AvgRatingDTO> GetAvgRestaurantRatingAsync(Guid restaurantId)
    {
        Console.WriteLine($"[Review_Service] Getting avg rating for restaurant: {restaurantId}");
        try 
        {
            var reviews = await db.Reviews
                .Where(r => r.RestaurantId == restaurantId && r.IsActive)
                .Select(r => r.FoodRating)
                .ToListAsync();
            
            Console.WriteLine($"[Review_Service] Found {reviews.Count} reviews for restaurant: {restaurantId}");
            return new AvgRatingDTO
            {
                AverageRating = reviews.Count == 0 ? 0 : Math.Round(reviews.Average(), 2),
                TotalReviews  = reviews.Count
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Review_Service] ERROR getting avg rating: {ex.Message}");
            throw;
        }
    }

    public async Task<AvgRatingDTO> GetAvgAgentRatingAsync(Guid agentId)
    {
        var reviews = await db.Reviews
            .Where(r => r.AgentId == agentId && r.IsActive)
            .Select(r => r.DeliveryRating)
            .ToListAsync();

        return new AvgRatingDTO
        {
            AverageRating = reviews.Count == 0 ? 0 : Math.Round(reviews.Average(), 2),
            TotalReviews  = reviews.Count
        };
    }

    private static ReviewDTOs Map(Review r) => new()
    {
        ReviewId       = r.ReviewId,
        OrderId        = r.OrderId,
        CustomerId     = r.CustomerId,
        RestaurantId   = r.RestaurantId,
        AgentId        = r.AgentId,
        FoodRating     = r.FoodRating,
        DeliveryRating = r.DeliveryRating,
        Comment        = r.Comment,
        CreatedAt      = r.CreatedAt
    };
}
