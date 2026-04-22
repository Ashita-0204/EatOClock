using Order_Service.DTOs;

namespace Order_Service.Interfaces;

public interface IOrderService
{
    Task<OrderDto> PlaceOrderAsync(string customerId, PlaceOrderRequest req);
    Task<OrderDto?> GetByIdAsync(Guid orderId, string callerId, string callerRole);
    Task<List<OrderDto>> GetCustomerOrdersAsync(string customerId);
    Task<List<OrderDto>> GetRestaurantOrdersAsync(Guid restaurantId);
    Task<List<OrderDto>> GetAllOrdersAsync();
    Task<OrderDto> UpdateStatusAsync(Guid orderId, UpdateStatusRequest req, string callerId, string callerRole);
    Task<OrderDto> CancelOrderAsync(Guid orderId, string customerId);
    Task<OrderDto> ReorderAsync(Guid orderId, string customerId);
    Task<OrderDto> AssignAgentAsync(Guid orderId, AssignAgentRequest req);
}
