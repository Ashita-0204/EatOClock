using Microsoft.EntityFrameworkCore;
using Order_Service.Data;
using Order_Service.DTOs;
using Order_Service.Interfaces;
using Order_Service.Models;

namespace Order_Service.Services;

public class OrderServiceImpl : IOrderService
{
    private readonly AppDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OrderServiceImpl(AppDbContext db, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    // -- Helpers --------------------------------------------------------------

    private static OrderDTOs ToDto(Order o) => new(
        o.OrderId, o.CustomerId, o.RestaurantId, o.RestaurantName, o.DeliveryAgentId,
        o.TotalAmount, o.Discount, o.FinalAmount, o.ModeOfPayment,
        o.Status.ToString(), o.DeliveryAddress, o.Notes, o.CancellationReason,
        o.Items.Select(i => new OrderItemDTO(
            i.OrderItemId, i.MenuItemId, i.Name, i.Price, i.Quantity,
            i.Customization, i.Price * i.Quantity)).ToList(),
        o.CreatedAt, o.UpdatedAt);

    // Remove AsNoTracking so that entities are attached for updates.
    private Task<Order?> FetchAsync(Guid id) =>
        _db.Orders
           .Include(o => o.Items)
           .FirstOrDefaultAsync(o => o.OrderId == id);

    private async Task<Order> RequireAsync(Guid id) =>
        await FetchAsync(id) ?? throw new KeyNotFoundException("Order not found.");

    // -- UC-29: Place order ---------------------------------------------------

    public async Task<OrderDTOs> PlaceOrderAsync(string customerId, PlaceOrderRequest req)
    {
        if (req.Items == null || req.Items.Count == 0)
            throw new InvalidOperationException("Order must have at least one item.");

        var mop = req.ModeOfPayment.Trim().ToUpper();
        if (!new[] { "COD", "WALLET", "ONLINE" }.Contains(mop))
            throw new InvalidOperationException("ModeOfPayment must be COD, Wallet, or Online.");

        var total    = req.Items.Sum(i => i.Price * i.Quantity);
        decimal disc = 0;

        var resName = "Unknown Restaurant";
        try
        {
            var client = _httpClientFactory.CreateClient("RestaurantService");
            var res = await client.GetFromJsonAsync<dynamic>($"/api/v1/restaurant/{req.RestaurantId}");
            if (res != null) resName = res.name;
        }
        catch { /* fallback */ }

        var order = new Order
        {
            CustomerId      = customerId,
            RestaurantId    = req.RestaurantId,
            RestaurantName  = resName,
            ModeOfPayment   = mop,
            DeliveryAddress = req.DeliveryAddress,
            Notes           = req.Notes,
            TotalAmount     = total,
            Discount        = disc,
            FinalAmount     = total - disc,
            Status          = OrderStatus.PLACED,
            Items           = req.Items.Select(i => new OrderItem
            {
                MenuItemId    = i.MenuItemId,
                Name          = i.Name,
                Price         = i.Price,
                Quantity      = i.Quantity,
                Customization = i.Customization
            }).ToList()
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        return ToDto(order);
    }

    // -- UC-30: Get order by id -----------------------------------------------

    public async Task<OrderDTOs?> GetByIdAsync(Guid orderId, string callerId, string callerRole)
    {
        var order = await RequireAsync(orderId);
        if (callerRole == "Customer" && order.CustomerId != callerId)
            throw new UnauthorizedAccessException("Access denied.");
        return ToDto(order);
    }

    // --  Order history -------------------------------------------------

    public async Task<List<OrderDTOs>> GetCustomerOrdersAsync(string customerId)
    {
        var orders = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return orders.Select(ToDto).ToList();
    }

    // --  Restaurant orders ---------------------------------------------

    public async Task<List<OrderDTOs>> GetRestaurantOrdersAsync(Guid restaurantId)
    {
        var orders = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.RestaurantId == restaurantId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return orders.Select(ToDto).ToList();
    }

    // --  Admin all orders ----------------------------------------------
    public async Task<List<OrderDTOs>> GetAllOrdersAsync()
    {
        var orders = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return orders.Select(ToDto).ToList();
    }

    // --  Agent available orders -------------------------------------------
    public async Task<List<OrderDTOs>> GetAvailableOrdersAsync()
    {
        var orders = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.Status == OrderStatus.READY_FOR_PICKUP && o.DeliveryAgentId == null)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync();
        return orders.Select(ToDto).ToList();
    }

    public async Task<List<OrderDTOs>> GetAgentOrdersAsync(string agentId)
    {
        var orders = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.DeliveryAgentId == agentId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return orders.Select(ToDto).ToList();
    }

    private static int GetStatusWeight(OrderStatus status) => status switch
    {
        OrderStatus.PLACED => 1,
        OrderStatus.CONFIRMED => 2,
        OrderStatus.PREPARING => 3,
        OrderStatus.READY_FOR_PICKUP => 4,
        OrderStatus.PICKED_UP => 5,
        OrderStatus.DELIVERED => 6,
        OrderStatus.CANCELLED => 99,
        _ => 0
    };

    // -- UC-30/34: Update status ----------------------------------------------

    public async Task<OrderDTOs> UpdateStatusAsync(
        Guid orderId, UpdateStatusRequest req, string callerId, string callerRole)
    {
        var order = await RequireAsync(orderId);

        if (callerRole != "Admin")
        {
            if (req.Status == order.Status) return ToDto(order); // No-op

            if (GetStatusWeight(req.Status) <= GetStatusWeight(order.Status) && req.Status != OrderStatus.CANCELLED)
                throw new InvalidOperationException(
                    $"Cannot move status from {order.Status} (weight {GetStatusWeight(order.Status)}) to {req.Status} (weight {GetStatusWeight(req.Status)}).");

            if (callerRole == "RestaurantOwner" &&
                !new[] { OrderStatus.CONFIRMED, OrderStatus.PREPARING, OrderStatus.READY_FOR_PICKUP }.Contains(req.Status))
                throw new InvalidOperationException($"RestaurantOwner cannot set status to {req.Status}. Allowed: CONFIRMED, PREPARING, READY_FOR_PICKUP.");

            if (callerRole == "DeliveryAgent" &&
                !new[] { OrderStatus.PICKED_UP, OrderStatus.DELIVERED }.Contains(req.Status))
                throw new InvalidOperationException($"DeliveryAgent cannot set status to {req.Status}. Allowed: PICKED_UP, DELIVERED.");
        }

        order.Status = req.Status;
        order.UpdatedAt = DateTime.UtcNow;

        if (req.Status == OrderStatus.READY_FOR_PICKUP && order.DeliveryAgentId == null)
        {
            // Auto assignment has been removed. 
            // Orders will remain unassigned until a Delivery Agent manually claims them.
        }

        await _db.SaveChangesAsync();

        return ToDto(order);
    }

    // -- UC-31: Cancel order --------------------------------------------------

    public async Task<OrderDTOs> CancelOrderAsync(Guid orderId, string customerId)
    {
        var order = await RequireAsync(orderId);

        if (order.CustomerId != customerId)
            throw new UnauthorizedAccessException("Cannot cancel another customer's order.");

        if (order.Status > OrderStatus.CONFIRMED)
            throw new InvalidOperationException(
                "Order cannot be cancelled after preparation has started.");

        order.Status = OrderStatus.CANCELLED;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return ToDto(order);
    }

    // -- UC-33: Reorder -------------------------------------------------------

    public async Task<OrderDTOs> ReorderAsync(Guid originalOrderId, string customerId)
    {
        var original = await RequireAsync(originalOrderId);

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

    // -- UC-36: Assign agent --------------------------------------------------

    public async Task<OrderDTOs> AssignAgentAsync(Guid orderId, AssignAgentRequest req)
    {
        var order = await RequireAsync(orderId);

        order.DeliveryAgentId = req.DeliveryAgentId;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return ToDto(order);
    }

    public async Task<OrderDTOs> ConfirmOrderAsync(Guid orderId)
    {
        var order = await RequireAsync(orderId);
        
        // Only allow confirming if status is PLACED
        if (order.Status != OrderStatus.PLACED)
            throw new InvalidOperationException($"Cannot confirm order in {order.Status} status.");

        order.Status = OrderStatus.CONFIRMED;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return ToDto(order);
    }
}