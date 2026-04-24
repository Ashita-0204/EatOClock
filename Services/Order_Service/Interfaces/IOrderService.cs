using Order_Service.DTOs;

namespace Order_Service.Interfaces;

public interface IOrderService
{
    Task<OrderDTOs> PlaceOrderAsync(string customerId, PlaceOrderRequest req);
    Task<OrderDTOs?> GetByIdAsync(Guid orderId, string callerId, string callerRole);
    Task<List<OrderDTOs>> GetCustomerOrdersAsync(string customerId);
    Task<List<OrderDTOs>> GetRestaurantOrdersAsync(Guid restaurantId);
    Task<List<OrderDTOs>> GetAllOrdersAsync();
    Task<OrderDTOs> UpdateStatusAsync(Guid orderId, UpdateStatusRequest req, string callerId, string callerRole);
    Task<OrderDTOs> CancelOrderAsync(Guid orderId, string customerId);
    Task<OrderDTOs> ReorderAsync(Guid orderId, string customerId);
    Task<OrderDTOs> AssignAgentAsync(Guid orderId, AssignAgentRequest req);
}
