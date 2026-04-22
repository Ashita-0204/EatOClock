using Cart_Service.DTOs;

namespace Cart_Service.Interfaces;

public interface ICartService
{
    Task<CartDto?> GetCartAsync(string customerId);
    Task<CartDto> AddItemAsync(string customerId, AddItemRequest req);
    Task<CartDto> UpdateQtyAsync(string customerId, Guid itemId, int qty);
    Task<CartDto> RemoveItemAsync(string customerId, Guid itemId);
    Task ClearCartAsync(string customerId);
    Task<CartDto> ApplyPromoAsync(string customerId, string promoCode);
    Task<CartDto> SwitchRestaurantAsync(string customerId, Guid newRestaurantId);
}
