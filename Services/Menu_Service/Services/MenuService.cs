using Microsoft.EntityFrameworkCore;
using Menu_Service.Data;
using Menu_Service.DTOs;
using Menu_Service.Interfaces;
using Menu_Service.Models;

namespace Menu_Service.Services;

public class MenuService : IMenuService
{
    private readonly AppDbContext _db;

    public MenuService(AppDbContext db) => _db = db;

    // -- Categories ----------------------------------------

    public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest req, string ownerId)
    {
        var category = new MenuCategory
        {
            RestaurantId = req.RestaurantId,
            Name = req.Name,
            Description = req.Description,
            DisplayOrder = req.DisplayOrder
        };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return MapCategory(category);
    }

    public async Task<List<CategoryResponse>> GetCategoriesByRestaurantAsync(Guid restaurantId)
    {
        var cats = await _db.Categories
            .Include(c => c.Items)
            .Where(c => c.RestaurantId == restaurantId && c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
        return cats.Select(MapCategory).ToList();
    }

    // -- Items ---------------------------------------------

    public async Task<MenuItemResponse> CreateItemAsync(CreateMenuItemRequest req, string ownerId)
    {
        var category = await _db.Categories.FindAsync(req.CategoryId)
            ?? throw new KeyNotFoundException("Category not found");

        var item = new MenuItem
        {
            CategoryId = req.CategoryId,
            RestaurantId = req.RestaurantId,
            Name = req.Name,
            Description = req.Description,
            Price = req.Price,
            ImageUrl = req.ImageUrl,
            IsVeg = req.IsVeg
        };
        _db.Items.Add(item);
        await _db.SaveChangesAsync();
        return MapItem(item);
    }

    public async Task<MenuItemResponse> UpdateItemAsync(Guid itemId, UpdateMenuItemRequest req, string ownerId)
    {
        var item = await _db.Items.FindAsync(itemId)
            ?? throw new KeyNotFoundException("Item not found");

        if (req.Name is not null) item.Name = req.Name;
        if (req.Description is not null) item.Description = req.Description;
        if (req.Price.HasValue) item.Price = req.Price.Value;
        if (req.ImageUrl is not null) item.ImageUrl = req.ImageUrl;
        if (req.IsVeg.HasValue) item.IsVeg = req.IsVeg.Value;
        if (req.CategoryId.HasValue) item.CategoryId = req.CategoryId.Value;
        item.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapItem(item);
    }

    public async Task DeleteItemAsync(Guid itemId, string ownerId)
    {
        var item = await _db.Items.FindAsync(itemId)
            ?? throw new KeyNotFoundException("Item not found");
        _db.Items.Remove(item);
        await _db.SaveChangesAsync();
    }

    public async Task<MenuItemResponse> ToggleAvailabilityAsync(Guid itemId, string ownerId)
    {
        var item = await _db.Items.FindAsync(itemId)
            ?? throw new KeyNotFoundException("Item not found");
        item.IsAvailable = !item.IsAvailable;
        item.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapItem(item);
    }

    // -- Mappers -------------------------------------------

    private static CategoryResponse MapCategory(MenuCategory c) => new(
        c.Id, c.RestaurantId, c.Name, c.Description, c.DisplayOrder, c.IsActive, c.CreatedAt,
        c.Items.Select(MapItem).ToList()
    );

    private static MenuItemResponse MapItem(MenuItem i) => new(
        i.Id, i.CategoryId, i.RestaurantId, i.Name, i.Description,
        i.Price, i.ImageUrl, i.IsAvailable, i.IsVeg, i.CreatedAt
    );
}
