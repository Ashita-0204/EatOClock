using System.ComponentModel.DataAnnotations;

namespace Menu_Service.DTOs;

// ── Category ──────────────────────────────────────────────

public record CreateCategoryRequest(
    [Required] Guid RestaurantId,
    [Required, MaxLength(100)] string Name,
    string Description = "",
    int DisplayOrder = 0
);

public record CategoryResponse(
    Guid Id,
    Guid RestaurantId,
    string Name,
    string Description,
    int DisplayOrder,
    bool IsActive,
    DateTime CreatedAt,
    List<MenuItemResponse> Items
);

// ── Menu Item ─────────────────────────────────────────────

public record CreateMenuItemRequest(
    [Required] Guid CategoryId,
    [Required] Guid RestaurantId,
    [Required, MaxLength(150)] string Name,
    string Description = "",
    [Required, Range(0.01, double.MaxValue)] decimal Price = 0,
    string? ImageUrl = null,
    bool IsVeg = false
);

public record UpdateMenuItemRequest(
    [MaxLength(150)] string? Name,
    string? Description,
    [Range(0.01, double.MaxValue)] decimal? Price,
    string? ImageUrl,
    bool? IsVeg,
    Guid? CategoryId
);

public record MenuItemResponse(
    Guid Id,
    Guid CategoryId,
    Guid RestaurantId,
    string Name,
    string Description,
    decimal Price,
    string? ImageUrl,
    bool IsAvailable,
    bool IsVeg,
    DateTime CreatedAt
);
