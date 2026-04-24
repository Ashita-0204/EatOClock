using DeliveryAgent_Service.DTOs;
using DeliveryAgent_Service.Models;

namespace DeliveryAgent_Service.Interfaces;

public interface IAgentService
{
    Task<ApiResponse<AgentDTOs>> RegisterAsync(string userId, RegisterAgentRequest req);
    Task<ApiResponse<AgentDTOs>> GetByIdAsync(Guid agentId);
    Task<ApiResponse<AgentDTOs>> GetByUserIdAsync(string userId);
    Task<ApiResponse<bool>> VerifyAsync(Guid agentId);
    Task<ApiResponse<bool>> ToggleAvailabilityAsync(Guid agentId, string userId);
    Task<ApiResponse<bool>> UpdateLocationAsync(Guid agentId, string userId, UpdateLocationRequest req);
    Task<ApiResponse<DeliveryRecordDTO>> AssignOrderAsync(Guid agentId, AssignOrderRequest req);
    Task<ApiResponse<bool>> MarkPickedUpAsync(Guid agentId, string userId, Guid orderId);
    Task<ApiResponse<bool>> MarkDeliveredAsync(Guid agentId, string userId, Guid orderId);
    Task<ApiResponse<List<DeliveryRecordDTO>>> GetAssignedOrdersAsync(Guid agentId, string userId);
    Task<ApiResponse<List<DeliveryRecordDTO>>> GetEarningsAsync(Guid agentId, string userId);
    Task<ApiResponse<List<NearbyAgentDTO>>> GetNearbyAgentsAsync(double lat, double lng, double radiusKm);
    Task<ApiResponse<bool>> UpdateRatingAsync(Guid agentId, RateDeliveryRequest req);
}
