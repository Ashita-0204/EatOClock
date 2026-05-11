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
    private readonly ILogger<CartServiceImpl> _logger;

    private static readonly DistributedCacheEntryOptions _cacheOpts =
        new() { SlidingExpiration = TimeSpan.FromMinutes(30) };

    public CartServiceImpl(AppDbContext db, IDistributedCache cache, ILogger<CartServiceImpl> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    private static string CacheKey(string customerId) => $"cart:{customerId}";

    private static CartDTOs ToDto(Cart cart, decimal? discountedTotal = null, string? promo = null)
    {
        var items = cart.Items.Select(i => new CartItemDTO(
            i.ItemId, i.MenuItemId, i.Name, i.Price, i.Quantity,
            i.Customization, i.Price * i.Quantity)).ToList();

        return new CartDTOs(cart.CartId, cart.CustomerId, cart.RestaurantId,
            cart.TotalPrice, discountedTotal ?? cart.TotalPrice, promo, items, cart.UpdatedAt);
    }

    private async Task InvalidateCacheAsync(string customerId)
    {
        try { await _cache.RemoveAsync(CacheKey(customerId)); }
        catch { }
    }

    private async Task<Cart> FetchFreshCartAsync(Guid cartId)
    {
        return await _db.Carts
            .AsNoTracking()
            .Include(c => c.Items)
            .FirstAsync(c => c.CartId == cartId);
    }

    public async Task<CartDTOs?> GetCartAsync(string customerId)
    {
        try
        {
            var cached = await _cache.GetStringAsync(CacheKey(customerId));
            if (cached != null)
                return JsonSerializer.Deserialize<CartDTOs>(cached);
        }
        catch { }

        var cart = await _db.Carts
            .AsNoTracking()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);

        if (cart == null)
        {
            _logger.LogInformation("[CART_DEBUG] GetCartAsync: No cart found in DB for customer {CustomerId}", customerId);
            return null;
        }

        _logger.LogInformation("[CART_DEBUG] GetCartAsync: Found cart {CartId} for restaurant {RestaurantId} with {ItemCount} items", cart.CartId, cart.RestaurantId, cart.Items.Count);
        var dto = ToDto(cart);
        try { await _cache.SetStringAsync(CacheKey(customerId), JsonSerializer.Serialize(dto), _cacheOpts); }
        catch { }
        return dto;
    }

    public async Task<CartDTOs> AddItemAsync(string customerId, AddItemRequest req)
    {
        _logger.LogInformation("[CART_DEBUG] AddItemAsync START: Customer={CustomerId}, ReqRest={RequestRestaurantId}", customerId, req.RestaurantId);

        // Always start with a clean tracker to prevent any stale entity state
        _db.ChangeTracker.Clear();

        // Step 1: Read existing cart (no tracking)
        var cart = await _db.Carts
            .AsNoTracking()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);

        Guid cartId;

        if (cart == null)
        {
            _logger.LogInformation("[CART_DEBUG] AddItemAsync: No existing cart. Creating new for {RestaurantId}", req.RestaurantId);
            cartId = Guid.NewGuid();
            var now = DateTime.UtcNow;
            await _db.Database.ExecuteSqlRawAsync(
                @"INSERT INTO ""Carts"" (""CartId"",""CustomerId"",""RestaurantId"",""TotalPrice"",""CreatedAt"",""UpdatedAt"")
                  VALUES ({0},{1},{2},{3},{4},{5})",
                cartId, customerId, req.RestaurantId, 0m, now, now);
        }
        else if (cart.RestaurantId != req.RestaurantId)
        {
            _logger.LogInformation("[CART_DEBUG] AddItemAsync: RESTAURANT MISMATCH. Cart={CartRest}, Req={ReqRest}, ItemsCount={ItemsCount}", cart.RestaurantId, req.RestaurantId, cart.Items.Count);
            if (cart.Items.Count == 0)
            {
                _logger.LogInformation("[CART_DEBUG] AddItemAsync: Cart is empty. HIJACKING to {NewRest}", req.RestaurantId);
                await _db.Database.ExecuteSqlRawAsync(
                    @"UPDATE ""Carts"" SET ""RestaurantId""={0}, ""UpdatedAt""={1} WHERE ""CartId""={2}",
                    req.RestaurantId, DateTime.UtcNow, cart.CartId);
                cartId = cart.CartId;
            }
            else
            {
                _logger.LogWarning("[CART_DEBUG] AddItemAsync: CONFLICT THROWN. Cart belongs to {CartRest}", cart.RestaurantId);
                throw new InvalidOperationException(
                    $"Conflict: Cart belongs to restaurant {cart.RestaurantId}, " +
                    $"but you tried to add an item from {req.RestaurantId}. Clear cart first.");
            }
        }
        else
        {
            _logger.LogInformation("[CART_DEBUG] AddItemAsync: MATCH. Using existing cart {CartId} for restaurant {RestaurantId}", cart.CartId, cart.RestaurantId);
            cartId = cart.CartId;
        }

        // Step 2: Add or increment item via raw SQL (no SaveChangesAsync, no tracker)
        decimal newTotal;

        var existing = cart?.Items.FirstOrDefault(
            i => i.MenuItemId == req.MenuItemId && i.Customization == req.Customization);

        if (existing != null)
        {
            _logger.LogInformation("[CART_DEBUG] AddItemAsync: Incrementing existing item {ItemId}. OldQty={OldQty}, AddQty={AddQty}", existing.ItemId, existing.Quantity, req.Quantity);
            var newQty = existing.Quantity + req.Quantity;
            await _db.Database.ExecuteSqlRawAsync(
                @"UPDATE ""CartItems"" SET ""Quantity""={0} WHERE ""ItemId""={1}",
                newQty, existing.ItemId);

            newTotal = cart!.Items.Sum(i =>
                i.ItemId == existing.ItemId
                    ? i.Price * newQty
                    : i.Price * i.Quantity);
        }
        else
        {
            _logger.LogInformation("[CART_DEBUG] AddItemAsync: Inserting new item {MenuItemId}", req.MenuItemId);
            var itemId = Guid.NewGuid();
            await _db.Database.ExecuteSqlRawAsync(
                @"INSERT INTO ""CartItems"" (""ItemId"",""CartId"",""MenuItemId"",""Name"",""Price"",""Quantity"",""Customization"")
                  VALUES ({0},{1},{2},{3},{4},{5},{6})",
                itemId, cartId, req.MenuItemId, req.Name, req.Price, req.Quantity,
                req.Customization);

            newTotal = (cart?.Items.Sum(i => i.Price * i.Quantity) ?? 0m)
                       + (req.Price * req.Quantity);
        }

        // Step 3: Update cart total via raw SQL
        _logger.LogInformation("[CART_DEBUG] AddItemAsync: Updating cart {CartId} total to {NewTotal}", cartId, newTotal);
        var updatedAt = DateTime.UtcNow;
        await _db.Database.ExecuteSqlRawAsync(
            @"UPDATE ""Carts"" SET ""TotalPrice""={0},""UpdatedAt""={1} WHERE ""CartId""={2}",
            newTotal, updatedAt, cartId);

        await InvalidateCacheAsync(customerId);

        return ToDto(await FetchFreshCartAsync(cartId));
    }

    public async Task<CartDTOs> UpdateQtyAsync(string customerId, Guid itemId, int qty)
    {
        _db.ChangeTracker.Clear();

        var cart = await _db.Carts
            .AsNoTracking()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId)
            ?? throw new KeyNotFoundException("Cart not found.");

        _ = cart.Items.FirstOrDefault(i => i.ItemId == itemId)
            ?? throw new KeyNotFoundException("Item not in cart.");

        if (qty <= 0)
        {
            await _db.Database.ExecuteSqlRawAsync(
                @"DELETE FROM ""CartItems"" WHERE ""ItemId""={0}", itemId);
        }
        else
        {
            await _db.Database.ExecuteSqlRawAsync(
                @"UPDATE ""CartItems"" SET ""Quantity""={0} WHERE ""ItemId""={1}", qty, itemId);
        }

        var newTotal = cart.Items
            .Where(i => i.ItemId != itemId || qty > 0)
            .Sum(i => i.ItemId == itemId ? i.Price * qty : i.Price * i.Quantity);

        var updatedAt = DateTime.UtcNow;
        await _db.Database.ExecuteSqlRawAsync(
            @"UPDATE ""Carts"" SET ""TotalPrice""={0},""UpdatedAt""={1} WHERE ""CartId""={2}",
            newTotal, updatedAt, cart.CartId);

        await InvalidateCacheAsync(customerId);
        return ToDto(await FetchFreshCartAsync(cart.CartId));
    }

    public async Task<CartDTOs> RemoveItemAsync(string customerId, Guid itemId)
    {
        _db.ChangeTracker.Clear();

        var cart = await _db.Carts
            .AsNoTracking()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId)
            ?? throw new KeyNotFoundException("Cart not found.");

        _ = cart.Items.FirstOrDefault(i => i.ItemId == itemId)
            ?? throw new KeyNotFoundException("Item not found in cart.");

        await _db.Database.ExecuteSqlRawAsync(
            @"DELETE FROM ""CartItems"" WHERE ""ItemId""={0}", itemId);

        var newTotal = cart.Items
            .Where(i => i.ItemId != itemId)
            .Sum(i => i.Price * i.Quantity);

        var updatedAt = DateTime.UtcNow;
        await _db.Database.ExecuteSqlRawAsync(
            @"UPDATE ""Carts"" SET ""TotalPrice""={0},""UpdatedAt""={1} WHERE ""CartId""={2}",
            newTotal, updatedAt, cart.CartId);

        await InvalidateCacheAsync(customerId);
        return ToDto(await FetchFreshCartAsync(cart.CartId));
    }

    public async Task ClearCartAsync(string customerId)
    {
        // FK cascade deletes CartItems automatically
        await _db.Database.ExecuteSqlRawAsync(
            @"DELETE FROM ""Carts"" WHERE ""CustomerId""={0}", customerId);

        await InvalidateCacheAsync(customerId);
    }

    public async Task<CartDTOs> ApplyPromoAsync(string customerId, string promoCode)
    {
        _db.ChangeTracker.Clear();

        var cart = await _db.Carts
            .AsNoTracking()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId)
            ?? throw new KeyNotFoundException("Cart not found.");

        var promo = await _db.PromoCodes
            .AsNoTracking()
            .FirstOrDefaultAsync(p =>
                p.Code == promoCode.ToUpper() && p.IsActive && p.ExpiresAt > DateTime.UtcNow)
            ?? throw new InvalidOperationException("Invalid or expired promo code.");

        var discounted = cart.TotalPrice * (1 - promo.DiscountPercent / 100m);
        return ToDto(cart, discounted, promo.Code);
    }

    public async Task<CartDTOs> SwitchRestaurantAsync(string customerId, Guid newRestaurantId)
    {
        await ClearCartAsync(customerId);

        var cartId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        await _db.Database.ExecuteSqlRawAsync(
            @"INSERT INTO ""Carts"" (""CartId"",""CustomerId"",""RestaurantId"",""TotalPrice"",""CreatedAt"",""UpdatedAt"")
              VALUES ({0},{1},{2},{3},{4},{5})",
            cartId, customerId, newRestaurantId, 0m, now, now);

        return ToDto(await FetchFreshCartAsync(cartId));
    }
}