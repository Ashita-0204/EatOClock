using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DeliveryAgent_Service.Hubs;

[Authorize]
public class LocationHub : Hub
{
    // Agent calls this to broadcast their location to anyone tracking the order
    public async Task UpdateLocation(string orderId, double latitude, double longitude)
    {
        await Clients.Group($"order_{orderId}")
            .SendAsync("LocationUpdated", new
            {
                AgentId = Context.UserIdentifier,
                Latitude = latitude,
                Longitude = longitude,
                Timestamp = DateTime.UtcNow
            });
    }

    // Customer/restaurant joins a group to track an order
    public async Task TrackOrder(string orderId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");

    public async Task StopTracking(string orderId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order_{orderId}");
}
