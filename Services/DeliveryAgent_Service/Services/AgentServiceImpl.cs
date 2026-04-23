using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using DeliveryAgent_Service.Data;
using DeliveryAgent_Service.DTOs;
using DeliveryAgent_Service.Hubs;
using DeliveryAgent_Service.Interfaces;
using DeliveryAgent_Service.Models;

namespace DeliveryAgent_Service.Services;

public class AgentServiceImpl(AppDbContext db, IHubContext<LocationHub> hub) : IAgentService
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static AgentDto ToDto(DeliveryAgent a) => new(
        a.AgentId, a.UserId, a.FullName, a.Phone, a.Email,
        a.VehicleType.ToString(), a.VehicleNumber,
        a.CurrentLatitude, a.CurrentLongitude,
        a.IsAvailable, a.IsVerified,
        a.AverageRating, a.TotalDeliveries, a.TotalEarnings);

    private static DeliveryRecordDto ToDto(DeliveryRecord d) => new(
        d.DeliveryId, d.OrderId, d.PickupAddress, d.DeliveryAddress,
        d.EarningsForDelivery, d.Status.ToString(), d.Rating,
        d.AssignedAt, d.PickedUpAt, d.DeliveredAt);

    // Haversine formula – returns distance in km
    private static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180)
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    // ── UC-43: Register ───────────────────────────────────────────────────────

    public async Task<ApiResponse<AgentDto>> RegisterAsync(string userId, RegisterAgentRequest req)
    {
        if (await db.DeliveryAgents.AnyAsync(a => a.UserId == userId))
            return new(false, "Agent already registered.", null);

        var agent = new DeliveryAgent
        {
            UserId = userId,
            FullName = req.FullName,
            Phone = req.Phone,
            Email = req.Email,
            VehicleType = req.VehicleType,
            VehicleNumber = req.VehicleNumber
        };

        db.DeliveryAgents.Add(agent);
        await db.SaveChangesAsync();
        return new(true, "Registered successfully. Await admin verification.", ToDto(agent));
    }

    // ── UC-43 / Get profile ───────────────────────────────────────────────────

    public async Task<ApiResponse<AgentDto>> GetByIdAsync(Guid agentId)
    {
        var a = await db.DeliveryAgents.FindAsync(agentId);
        return a is null
            ? new(false, "Agent not found.", null)
            : new(true, null, ToDto(a));
    }

    public async Task<ApiResponse<AgentDto>> GetByUserIdAsync(string userId)
    {
        var a = await db.DeliveryAgents.FirstOrDefaultAsync(x => x.UserId == userId);
        return a is null
            ? new(false, "Agent not found.", null)
            : new(true, null, ToDto(a));
    }

    // ── UC-44: Admin verifies agent ───────────────────────────────────────────

    public async Task<ApiResponse<bool>> VerifyAsync(Guid agentId)
    {
        var a = await db.DeliveryAgents.FindAsync(agentId);
        if (a is null) return new(false, "Agent not found.", false);

        a.IsVerified = true;
        await db.SaveChangesAsync();
        return new(true, "Agent verified.", true);
    }

    // ── UC-45: Toggle availability ────────────────────────────────────────────

    public async Task<ApiResponse<bool>> ToggleAvailabilityAsync(Guid agentId, string userId)
    {
        var a = await db.DeliveryAgents.FindAsync(agentId);
        if (a is null || a.UserId != userId) return new(false, "Not found.", false);
        if (!a.IsVerified) return new(false, "Account not verified yet.", false);

        a.IsAvailable = !a.IsAvailable;
        await db.SaveChangesAsync();
        return new(true, $"Now {(a.IsAvailable ? "online" : "offline")}.", a.IsAvailable);
    }

    // ── UC-47: Update GPS location ────────────────────────────────────────────

    public async Task<ApiResponse<bool>> UpdateLocationAsync(Guid agentId, string userId, UpdateLocationRequest req)
    {
        var a = await db.DeliveryAgents.FindAsync(agentId);
        if (a is null || a.UserId != userId) return new(false, "Not found.", false);

        a.CurrentLatitude = req.Latitude;
        a.CurrentLongitude = req.Longitude;
        await db.SaveChangesAsync();

        // Broadcast via SignalR to any active delivery group
        var activeDelivery = await db.DeliveryRecords
            .Where(d => d.AgentId == agentId && d.Status == DeliveryStatus.PickedUp)
            .FirstOrDefaultAsync();

        if (activeDelivery is not null)
            await hub.Clients.Group($"order_{activeDelivery.OrderId}")
                .SendAsync("LocationUpdated", new { AgentId = agentId, req.Latitude, req.Longitude, Timestamp = DateTime.UtcNow });

        return new(true, "Location updated.", true);
    }

    // ── System: Assign order to agent ─────────────────────────────────────────

    public async Task<ApiResponse<DeliveryRecordDto>> AssignOrderAsync(Guid agentId, AssignOrderRequest req)
    {
        var a = await db.DeliveryAgents.FindAsync(agentId);
        if (a is null) return new(false, "Agent not found.", null);
        if (!a.IsVerified || !a.IsAvailable) return new(false, "Agent unavailable.", null);

        var record = new DeliveryRecord
        {
            AgentId = agentId,
            OrderId = req.OrderId,
            CustomerId = req.CustomerId,
            PickupAddress = req.PickupAddress,
            DeliveryAddress = req.DeliveryAddress,
            EarningsForDelivery = req.EarningsForDelivery
        };

        a.IsAvailable = false;  // mark busy
        db.DeliveryRecords.Add(record);
        await db.SaveChangesAsync();
        return new(true, "Order assigned.", ToDto(record));
    }

    // ── UC-48: Mark picked up ─────────────────────────────────────────────────

    public async Task<ApiResponse<bool>> MarkPickedUpAsync(Guid agentId, string userId, Guid orderId)
    {
        var record = await db.DeliveryRecords
            .Include(d => d.Agent)
            .FirstOrDefaultAsync(d => d.AgentId == agentId && d.OrderId == orderId);

        if (record is null || record.Agent.UserId != userId)
            return new(false, "Delivery not found.", false);

        record.Status = DeliveryStatus.PickedUp;
        record.PickedUpAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        await hub.Clients.Group($"order_{orderId}")
            .SendAsync("StatusChanged", new { OrderId = orderId, Status = "PickedUp" });

        return new(true, "Marked as picked up.", true);
    }

    // ── UC-48: Mark delivered ─────────────────────────────────────────────────

    public async Task<ApiResponse<bool>> MarkDeliveredAsync(Guid agentId, string userId, Guid orderId)
    {
        var record = await db.DeliveryRecords
            .Include(d => d.Agent)
            .FirstOrDefaultAsync(d => d.AgentId == agentId && d.OrderId == orderId);

        if (record is null || record.Agent.UserId != userId)
            return new(false, "Delivery not found.", false);

        record.Status = DeliveryStatus.Delivered;
        record.DeliveredAt = DateTime.UtcNow;

        var agent = record.Agent;
        agent.TotalDeliveries++;
        agent.TotalEarnings += record.EarningsForDelivery;
        agent.IsAvailable = true;

        await db.SaveChangesAsync();

        await hub.Clients.Group($"order_{orderId}")
            .SendAsync("StatusChanged", new { OrderId = orderId, Status = "Delivered" });

        return new(true, "Marked as delivered.", true);
    }

    // ── UC-46: View assigned orders ───────────────────────────────────────────

    public async Task<ApiResponse<List<DeliveryRecordDto>>> GetAssignedOrdersAsync(Guid agentId, string userId)
    {
        var agent = await db.DeliveryAgents.FindAsync(agentId);
        if (agent is null || agent.UserId != userId) return new(false, "Not found.", null);

        var records = await db.DeliveryRecords
            .Where(d => d.AgentId == agentId && d.Status != DeliveryStatus.Delivered)
            .OrderByDescending(d => d.AssignedAt)
            .Select(d => ToDto(d))
            .ToListAsync();

        return new(true, null, records);
    }

    // ── UC-49: View earnings ──────────────────────────────────────────────────

    public async Task<ApiResponse<List<DeliveryRecordDto>>> GetEarningsAsync(Guid agentId, string userId)
    {
        var agent = await db.DeliveryAgents.FindAsync(agentId);
        if (agent is null || agent.UserId != userId) return new(false, "Not found.", null);

        var records = await db.DeliveryRecords
            .Where(d => d.AgentId == agentId && d.Status == DeliveryStatus.Delivered)
            .OrderByDescending(d => d.DeliveredAt)
            .Select(d => ToDto(d))
            .ToListAsync();

        return new(true, null, records);
    }

    // ── UC-50: Geo proximity lookup ───────────────────────────────────────────

    public async Task<ApiResponse<List<NearbyAgentDto>>> GetNearbyAgentsAsync(double lat, double lng, double radiusKm)
    {
        var agents = await db.DeliveryAgents
            .Where(a => a.IsAvailable && a.IsVerified
                     && a.CurrentLatitude != null && a.CurrentLongitude != null)
            .ToListAsync();

        var nearby = agents
            .Select(a => new
            {
                Agent = a,
                Dist = Haversine(lat, lng, a.CurrentLatitude!.Value, a.CurrentLongitude!.Value)
            })
            .Where(x => x.Dist <= radiusKm)
            .OrderBy(x => x.Dist)
            .Select(x => new NearbyAgentDto(
                x.Agent.AgentId, x.Agent.UserId, x.Agent.FullName,
                x.Agent.VehicleType.ToString(),
                x.Agent.CurrentLatitude!.Value, x.Agent.CurrentLongitude!.Value,
                Math.Round(x.Dist, 2)))
            .ToList();

        return new(true, null, nearby);
    }

    // ── System: Update rating ─────────────────────────────────────────────────

    public async Task<ApiResponse<bool>> UpdateRatingAsync(Guid agentId, RateDeliveryRequest req)
    {
        var record = await db.DeliveryRecords.FindAsync(req.DeliveryId);
        if (record is null || record.AgentId != agentId) return new(false, "Delivery not found.", false);

        record.Rating = req.Rating;
        record.RatingNote = req.Note;

        // Recalculate average
        var agent = await db.DeliveryAgents.FindAsync(agentId);
        if (agent is not null)
        {
            var ratings = await db.DeliveryRecords
                .Where(d => d.AgentId == agentId && d.Rating != null)
                .Select(d => d.Rating!.Value)
                .ToListAsync();

            agent.AverageRating = ratings.Count > 0 ? ratings.Average() : 0;
        }

        await db.SaveChangesAsync();
        return new(true, "Rating updated.", true);
    }
}
