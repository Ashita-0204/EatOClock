using Microsoft.EntityFrameworkCore;
using Order_Service.Data;
using Order_Service.DTOs;
using Order_Service.Interfaces;
using Order_Service.Models;

namespace Order_Service.Services;

public class OrderServiceImpl : IOrderService
{
    private readonly AppDbContext _db;

    public OrderServiceImpl(AppDbContext db) => _db = db;

    // ─── helpers ──────────────────────────────────────────────────────────

    private static OrderDto ToDto(Order o) => new(
        o.OrderId, o.CustomerId, o.RestaurantId, o.DeliveryAgentId,
        o.TotalAmount, o.Discount, o.FinalAmount, o.ModeOfPayment,
        o.Status.ToString(), o.DeliveryAddress, o.Notes, o.CancellationReason,
        o.Items.Select(i => new OrderItemDto(
            i.OrderItemId, i.MenuItemId, i.Name, i.Price, i.Quantity,
            i.Customization, i.Price * i.Quantity)).ToList(),
        o.CreatedAt, o.UpdatedAt);

    private async Task<Order> LoadAsync(Guid id) =>
        await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.OrderId == id)
        ?? throw new KeyNotFoundException("Order not found.");

    // ─── UC-29: Place order ────────────────────────────────────────────────

    public async Task<OrderDto> PlaceOrderAsync(string customerId, PlaceOrderRequest req)
    {
        if (req.Items == null || req.Items.Count == 0)
            throw new InvalidOperationException("Order must have at least one item.");

        if (!new[] { "COD", "Online" }.Contains(req.ModeOfPayment, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("ModeOfPayment must be COD or Online.");

        var total = req.Items.Sum(i => i.Price * i.Quantity);
        // Simple discount placeholder — extend with promo logic as needed
        decimal discount = 0;

        var order = new Order
        {
            CustomerId = customerId,
            RestaurantId = req.RestaurantId,
            ModeOfPayment = req.ModeOfPayment.ToUpper(),
            DeliveryAddress = req.DeliveryAddress,
            Notes = req.Notes,
            TotalAmount = total,
            Discount = discount,
            FinalAmount = total - discount,
            Status = OrderStatus.PLACED,
            Items = req.Items.Select(i => new OrderItem
            {
                MenuItemId = i.MenuItemId,
                Name = i.Name,
                Price = i.Price,
                Quantity = i.Quantity,
                Customization = i.Customization
            }).ToList()
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        return ToDto(order);
    }

    // ─── UC-30: Get order by id ────────────────────────────────────────────

    public async Task<OrderDto?> GetByIdAsync(Guid orderId, string callerId, string callerRole)
    {
        var order = await LoadAsync(orderId);

        // Customers can only see their own orders; owners/agents/admins can see all
        if (callerRole == "Customer" && order.CustomerId != callerId)
            throw new UnauthorizedAccessException("Access denied.");

        return ToDto(order);
    }

    // ─── UC-32: Order history ──────────────────────────────────────────────

    public async Task<List<OrderDto>> GetCustomerOrdersAsync(string customerId)
    {
        var orders = await _db.Orders.Include(o => o.Items)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return orders.Select(ToDto).ToList();
    }

    // ─── UC-34: Restaurant orders ──────────────────────────────────────────

    public async Task<List<OrderDto>> GetRestaurantOrdersAsync(Guid restaurantId)
    {
        var orders = await _db.Orders.Include(o => o.Items)
            .Where(o => o.RestaurantId == restaurantId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return orders.Select(ToDto).ToList();
    }

    // ─── UC-35: Admin all orders ───────────────────────────────────────────

    public async Task<List<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _db.Orders.Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return orders.Select(ToDto).ToList();
    }

    // ─── UC-30/34: Update status ───────────────────────────────────────────

    public async Task<OrderDto> UpdateStatusAsync(Guid orderId, UpdateStatusRequest req, string callerId, string callerRole)
    {
        var order = await LoadAsync(orderId);

        // Validate forward-only transitions
        if ((int)req.Status <= (int)order.Status && req.Status != OrderStatus.CANCELLED)
            throw new InvalidOperationException($"Cannot move status from {order.Status} to {req.Status}.");

        // Restaurant owners can only update their own restaurant orders
        if (callerRole == "RestaurantOwner")
        {
            // RestaurantId check would require restaurant-service call; trust JWT sub for now
            if (!new[] { OrderStatus.CONFIRMED, OrderStatus.PREPARING }.Contains(req.Status))
                throw new InvalidOperationException("Restaurant can only set CONFIRMED or PREPARING.");
        }

        if (callerRole == "DeliveryAgent")
        {
            if (!new[] { OrderStatus.PICKED_UP, OrderStatus.DELIVERED }.Contains(req.Status))
                throw new InvalidOperationException("Agent can only set PICKED_UP or DELIVERED.");
        }

        order.Status = req.Status;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToDto(order);
    }

    // ─── UC-31: Cancel order ───────────────────────────────────────────────

    public async Task<OrderDto> CancelOrderAsync(Guid orderId, string customerId)
    {
        var order = await LoadAsync(orderId);

        if (order.CustomerId != customerId)
            throw new UnauthorizedAccessException("Cannot cancel another customer's order.");

        if (order.Status > OrderStatus.CONFIRMED)
            throw new InvalidOperationException("Order cannot be cancelled after preparation has started.");

        order.Status = OrderStatus.CANCELLED;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToDto(order);
    }

    // ─── UC-33: Reorder ───────────────────────────────────────────────────

    public async Task<OrderDto> ReorderAsync(Guid originalOrderId, string customerId)
    {
        var original = await LoadAsync(originalOrderId);

        if (original.CustomerId != customerId)
            throw new UnauthorizedAccessException("Access denied.");

        var req = new PlaceOrderRequest(
            original.RestaurantId,
            original.ModeOfPayment,
            original.DeliveryAddress,
            original.Notes,
            null,
            original.Items.Select(i => new OrderItemRequest(
                i.MenuItemId, i.Name, i.Price, i.Quantity, i.Customization)).ToList());

        return await PlaceOrderAsync(customerId, req);
    }

    // ─── UC-36: Assign agent ───────────────────────────────────────────────

    public async Task<OrderDto> AssignAgentAsync(Guid orderId, AssignAgentRequest req)
    {
        var order = await LoadAsync(orderId);
        order.DeliveryAgentId = req.DeliveryAgentId;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToDto(order);
    }
}
