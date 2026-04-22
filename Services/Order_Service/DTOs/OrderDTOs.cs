using Order_Service.Models;

namespace Order_Service.DTOs;

// ─── Requests ───────────────────────────────────────────────────────────────

public record PlaceOrderRequest(
    Guid RestaurantId,
    string ModeOfPayment,           // "COD" | "Online"
    string DeliveryAddress,
    string? Notes,
    string? PromoCode,
    List<OrderItemRequest> Items);

public record OrderItemRequest(
    Guid MenuItemId,
    string Name,
    decimal Price,
    int Quantity,
    string? Customization);

public record UpdateStatusRequest(OrderStatus Status);

public record CancelOrderRequest(string? Reason);

public record AssignAgentRequest(string DeliveryAgentId);

// ─── Responses ──────────────────────────────────────────────────────────────

public record OrderItemDto(
    Guid OrderItemId,
    Guid MenuItemId,
    string Name,
    decimal Price,
    int Quantity,
    string? Customization,
    decimal Subtotal);

public record OrderDto(
    Guid OrderId,
    string CustomerId,
    Guid RestaurantId,
    string? DeliveryAgentId,
    decimal TotalAmount,
    decimal Discount,
    decimal FinalAmount,
    string ModeOfPayment,
    string Status,
    string DeliveryAddress,
    string? Notes,
    string? CancellationReason,
    List<OrderItemDto> Items,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record ApiResponse<T>(bool Success, string? Message, T? Data);
