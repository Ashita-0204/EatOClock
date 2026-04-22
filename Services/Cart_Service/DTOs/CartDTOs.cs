namespace Cart_Service.DTOs;

// ─── Requests ───────────────────────────────────────────────────────────────

public record AddItemRequest(
    Guid RestaurantId,
    Guid MenuItemId,
    string Name,
    decimal Price,
    int Quantity,
    string? Customization = null);

public record UpdateQtyRequest(int Quantity);

public record ApplyPromoRequest(string PromoCode);

public record SwitchRestaurantRequest(Guid NewRestaurantId);

// ─── Responses ──────────────────────────────────────────────────────────────

public record CartItemDto(
    Guid ItemId,
    Guid MenuItemId,
    string Name,
    decimal Price,
    int Quantity,
    string? Customization,
    decimal Subtotal);

public record CartDto(
    Guid CartId,
    string CustomerId,
    Guid RestaurantId,
    decimal TotalPrice,
    decimal DiscountedTotal,
    string? AppliedPromo,
    List<CartItemDto> Items,
    DateTime UpdatedAt);

public record ApiResponse<T>(bool Success, string? Message, T? Data);
