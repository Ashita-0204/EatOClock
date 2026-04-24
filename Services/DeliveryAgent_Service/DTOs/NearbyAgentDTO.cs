using DeliveryAgent_Service.Models;

namespace DeliveryAgent_Service.DTOs;

public class NearbyAgentDTO
{
    public Guid AgentId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double DistanceKm { get; set; }
public NearbyAgentDTO() { }

    public NearbyAgentDTO(Guid agentId, string userId, string fullName,
        string vehicleType, double latitude, double longitude, double distanceKm)
    {
        AgentId = agentId;
        UserId = userId;
        FullName = fullName;
        VehicleType = vehicleType;
        Latitude = latitude;
        Longitude = longitude;
        DistanceKm = distanceKm;
    }}
