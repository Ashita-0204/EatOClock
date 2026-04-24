using DeliveryAgent_Service.Models;

namespace DeliveryAgent_Service.DTOs;

public class AgentDTOs
{
    public Guid AgentId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public string VehicleNumber { get; set; } = string.Empty;
    public double? CurrentLatitude { get; set; }
    public double? CurrentLongitude { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsVerified { get; set; }
    public double AverageRating { get; set; }
    public int TotalDeliveries { get; set; }
    public decimal TotalEarnings { get; set; }
    public AgentDTOs() { }

    public AgentDTOs(Guid agentId, string userId, string fullName, string phone,
        string email, string vehicleType, string vehicleNumber,
        double? lat, double? lon, bool isAvailable, bool isVerified,
        double avgRating, int totalDeliveries, decimal totalEarnings)
    {
        AgentId = agentId;
        UserId = userId;
        FullName = fullName;
        Phone = phone;
        Email = email;
        VehicleType = vehicleType;
        VehicleNumber = vehicleNumber;
        CurrentLatitude = lat;
        CurrentLongitude = lon;
        IsAvailable = isAvailable;
        IsVerified = isVerified;
        AverageRating = avgRating;
        TotalDeliveries = totalDeliveries;
        TotalEarnings = totalEarnings;
    }
}
