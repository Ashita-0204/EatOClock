using Menu_Service.DTOs;

namespace Menu_Service.Interfaces;

public interface IMenuService
{
    // Categories
    Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request, string ownerId);
    Task<List<CategoryResponse>> GetCategoriesByRestaurantAsync(Guid restaurantId);

    // Items
    Task<MenuItemResponse> CreateItemAsync(CreateMenuItemRequest request, string ownerId);
    Task<MenuItemResponse> UpdateItemAsync(Guid itemId, UpdateMenuItemRequest request, string ownerId);
    Task DeleteItemAsync(Guid itemId, string ownerId);
    Task<MenuItemResponse> ToggleAvailabilityAsync(Guid itemId, string ownerId);
}
