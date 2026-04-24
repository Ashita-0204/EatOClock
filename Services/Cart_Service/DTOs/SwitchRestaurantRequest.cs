namespace Cart_Service.DTOs;

public class SwitchRestaurantRequest
{
    public Guid NewRestaurantId { get; set; }
     public SwitchRestaurantRequest(Guid newRestaurantId)
    {
        NewRestaurantId = newRestaurantId;
    }
}
