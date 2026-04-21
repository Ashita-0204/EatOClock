namespace Auth_Service.Enums;

/// <summary>
/// Roles a user can self-assign at registration.
/// Admin is intentionally excluded.
/// </summary>
public enum AllowedRegistrationRole
{
    Customer = 1,
    RestaurantOwner = 2,
    DeliveryAgent = 3
}