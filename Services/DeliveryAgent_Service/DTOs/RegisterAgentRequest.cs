using DeliveryAgent_Service.Models;

namespace DeliveryAgent_Service.DTOs;

// --- Requests ---------------------------------------------------------------

public class RegisterAgentRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public VehicleType VehicleType { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public RegisterAgentRequest() { }

    public RegisterAgentRequest(string fullName, string phone, string email,
        VehicleType vehicleType, string vehicleNumber)
    {
        FullName = fullName;
        Phone = phone;
        Email = email;
        VehicleType = vehicleType;
        VehicleNumber = vehicleNumber;
    }
}
