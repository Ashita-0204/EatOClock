using System.Text.Json;
using Cart_Service.Data;
using Cart_Service.DTOs;
using Cart_Service.Interfaces;
using Cart_Service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Cart_Service.Services;

public class CartServiceImpl : ICartService
{
    private readonly AppDbContext _db;
    private readonly IDistributedCache _cache;

    // Redis TTL - 30 minutes idle expiry
    private static readonly DistributedCacheEntryOptions _cacheOpts =
        new() { SlidingExpiration = TimeSpan.FromMinutes(30) };

    public CartServiceImpl(AppDbContext db, IDistributedCache cache)
    {
        _db = db;
        _cache = cache;
    }

    // --- helpers ------------------------------------------------------------

    private static string CacheKey(string customerId) => $"cart:{customerId}";

    private static CartDTOs ToDto(Cart cart, decimal? discountedTotal = null, string? promo = null)
    {
        var items = cart.Items.Select(i => new CartItemDTO(
            i.ItemId, i.MenuItemId, i.Name, i.Price, i.Quantity, i.Customization,
            i.Price * i.Quantity)).ToList();

        return new CartDTOs(cart.CartId, cart.CustomerId, cart.RestaurantId,
            cart.TotalPrice, discountedTotal ?? cart.TotalPrice, promo, items, cart.UpdatedAt);
    }

    private async Task<Cart?> LoadFromDbAsync(string customerId) =>
        await _db.Carts.Include(c => c.Items)
                       .FirstOrDefaultAsync(c => c.CustomerId == customerId);

    private async Task InvalidateCacheAsync(string customerId) =>
        await _cache.RemoveAsync(CacheKey(customerId));

    private void RecalcTotal(Cart cart) =>
        cart.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);

    // --- public methods ------------------------------------------------------

    public async Task<CartDTOs?> GetCartAsync(string customerId)
    {
        // try cache first
        var cached = await _cache.GetStringAsync(CacheKey(customerId));
        if (cached != null)
            return JsonSerializer.Deserialize<CartDTOs>(cached);

        var cart = await LoadFromDbAsync(customerId);
        if (cart == null) return null;

        var dto = ToDto(cart);
        await _cache.SetStringAsync(CacheKey(customerId), JsonSerializer.Serialize(dto), _cacheOpts);
        return dto;
    }

    public async Task<CartDTOs> AddItemAsync(string customerId, AddItemRequest req)
    {
        var cart = await LoadFromDbAsync(customerId);

        if (cart == null)
        {
            cart = new Cart { CustomerId = customerId, RestaurantId = req.RestaurantId };
            _db.Carts.Add(cart);
        }
        else if (cart.RestaurantId != req.RestaurantId)
        {
            throw new InvalidOperationException(
                $"Cart belongs to restaurant {cart.RestaurantId}. Use switch-restaurant to change.");
        }

        // merge if same menu item + customization
        var existing = cart.Items.FirstOrDefault(
            i => i.MenuItemId == req.MenuItemId && i.Customization == req.Customization);

        if (existing != null)
            existing.Quantity += req.Quantity;
        else
            cart.Items.Add(new CartItem
            {
                CartId       = cart.CartId,
                MenuItemId   = req.MenuItemId,
                Name         = req.Name,
                Price        = req.Price,
                Quantity     = req.Quantity,
                Customization = req.Customization
            });

        RecalcTotal(cart);
        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await InvalidateCacheAsync(customerId);
        return ToDto(cart);
    }

    public async Task<CartDTOs> UpdateQtyAsync(string customerId, Guid itemId, int qty)
    {
        var cart = await LoadFromDbAsync(customerId)
                   ?? throw new KeyNotFoundException("Cart not found.");

        var item = cart.Items.FirstOrDefault(i => i.ItemId == itemId)
                   ?? throw new KeyNotFoundException("Item not in cart.");

        if (qty <= 0)
            cart.Items.Remove(item);
        else
            item.Quantity = qty;

        RecalcTotal(cart);
        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await InvalidateCacheAsync(customerId);
        return ToDto(cart);
    }

    public async Task<CartDTOs> RemoveItemAsync(string customerId, Guid itemId)
    {
        var cart = await LoadFromDbAsync(customerId)
                   ?? throw new KeyNotFoundException("Cart not found.");

        var item = cart.Items.FirstOrDefault(i => i.ItemId == itemId)
                   ?? throw new KeyNotFoundException("Item not found.");

        cart.Items.Remove(item);
        RecalcTotal(cart);
        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await InvalidateCacheAsync(customerId);
        return ToDto(cart);
    }

    public async Task ClearCartAsync(string customerId)
    {
        var cart = await LoadFromDbAsync(customerId);
        if (cart != null)
        {
            _db.Carts.Remove(cart);
            await _db.SaveChangesAsync();
        }
        await InvalidateCacheAsync(customerId);
    }

    public async Task<CartDTOs> ApplyPromoAsync(string customerId, string promoCode)
    {
        var cart = await LoadFromDbAsync(customerId)
                   ?? throw new KeyNotFoundException("Cart not found.");

        var promo = await _db.PromoCodes
            .FirstOrDefaultAsync(p => p.Code == promoCode.ToUpper() &&
                                      p.IsActive && p.ExpiresAt > DateTime.UtcNow)
                   ?? throw new InvalidOperationException("Invalid or expired promo code.");

        var discounted = cart.TotalPrice * (1 - promo.DiscountPercent / 100m);
        // We return discounted total without persisting it (promo applied at checkout)
        return ToDto(cart, discounted, promo.Code);
    }

    public async Task<CartDTOs> SwitchRestaurantAsync(string customerId, Guid newRestaurantId)
    {
        await ClearCartAsync(customerId);

        var cart = new Cart { CustomerId = customerId, RestaurantId = newRestaurantId };
        _db.Carts.Add(cart);
        await _db.SaveChangesAsync();

        return ToDto(cart);
    }
}
