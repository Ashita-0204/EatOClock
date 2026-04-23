namespace DeliveryAgent_Service.Models;

public class DeliveryAgent
{
    public Guid AgentId { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;       // from Auth JWT (NameIdentifier)
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public VehicleType VehicleType { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;

    // Live location
    public double? CurrentLatitude { get; set; }
    public double? CurrentLongitude { get; set; }

    public bool IsAvailable { get; set; } = false;
    public bool IsVerified { get; set; } = false;

    public double AverageRating { get; set; } = 0;
    public int TotalDeliveries { get; set; } = 0;
    public decimal TotalEarnings { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<DeliveryRecord> Deliveries { get; set; } = new();
}

public class DeliveryRecord
{
    public Guid DeliveryId { get; set; } = Guid.NewGuid();
    public Guid AgentId { get; set; }
    public Guid OrderId { get; set; }

    public string CustomerId { get; set; } = string.Empty;
    public string PickupAddress { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public decimal EarningsForDelivery { get; set; }

    public DeliveryStatus Status { get; set; } = DeliveryStatus.Assigned;
    public int? Rating { get; set; }               // 1-5 given by customer
    public string? RatingNote { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    public DeliveryAgent Agent { get; set; } = null!;
}
