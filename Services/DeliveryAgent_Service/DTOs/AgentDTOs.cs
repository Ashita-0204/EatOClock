using DeliveryAgent_Service.Models;

namespace DeliveryAgent_Service.DTOs;

// ─── Requests ───────────────────────────────────────────────────────────────

public record RegisterAgentRequest(
    string FullName,
    string Phone,
    string Email,
    VehicleType VehicleType,
    string VehicleNumber);

public record UpdateLocationRequest(double Latitude, double Longitude);

public record AssignOrderRequest(
    Guid OrderId,
    string CustomerId,
    string PickupAddress,
    string DeliveryAddress,
    decimal EarningsForDelivery);

public record RateDeliveryRequest(Guid DeliveryId, int Rating, string? Note);

// ─── Responses ──────────────────────────────────────────────────────────────

public record AgentDto(
    Guid AgentId,
    string UserId,
    string FullName,
    string Phone,
    string Email,
    string VehicleType,
    string VehicleNumber,
    double? CurrentLatitude,
    double? CurrentLongitude,
    bool IsAvailable,
    bool IsVerified,
    double AverageRating,
    int TotalDeliveries,
    decimal TotalEarnings);

public record DeliveryRecordDto(
    Guid DeliveryId,
    Guid OrderId,
    string PickupAddress,
    string DeliveryAddress,
    decimal EarningsForDelivery,
    string Status,
    int? Rating,
    DateTime AssignedAt,
    DateTime? PickedUpAt,
    DateTime? DeliveredAt);

public record NearbyAgentDto(
    Guid AgentId,
    string UserId,
    string FullName,
    string VehicleType,
    double Latitude,
    double Longitude,
    double DistanceKm);

public record ApiResponse<T>(bool Success, string? Message, T? Data);
