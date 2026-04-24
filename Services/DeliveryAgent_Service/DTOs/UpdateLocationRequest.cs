using DeliveryAgent_Service.Models;

namespace DeliveryAgent_Service.DTOs;

public class UpdateLocationRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public UpdateLocationRequest() { }

    public UpdateLocationRequest(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }
}
