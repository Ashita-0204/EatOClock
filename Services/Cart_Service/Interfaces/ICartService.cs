using Cart_Service.DTOs;

namespace Cart_Service.Interfaces;

public interface ICartService
{
    Task<CartDTOs?> GetCartAsync(string customerId);
    Task<CartDTOs> AddItemAsync(string customerId, AddItemRequest req);
    Task<CartDTOs> UpdateQtyAsync(string customerId, Guid itemId, int qty);
    Task<CartDTOs> RemoveItemAsync(string customerId, Guid itemId);
    Task ClearCartAsync(string customerId);
    Task<CartDTOs> ApplyPromoAsync(string customerId, string promoCode);
    Task<CartDTOs> SwitchRestaurantAsync(string customerId, Guid newRestaurantId);
}
