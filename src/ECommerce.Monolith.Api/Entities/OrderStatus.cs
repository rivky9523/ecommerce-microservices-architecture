namespace ECommerce.Monolith.Api.Entities;

/// <summary>
/// The lifecycle states an order can be in for Phase 1.
/// Stored in the database as a string for readability.
/// </summary>
public enum OrderStatus
{
    Pending,
    Confirmed,
    Rejected
}
