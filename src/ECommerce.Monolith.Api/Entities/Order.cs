namespace ECommerce.Monolith.Api.Entities;

/// <summary>
/// A customer order. The total amount and status are computed by the
/// order service when the order is placed, not supplied by the client.
/// </summary>
public class Order
{
    public int Id { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }

    /// <summary>The line items belonging to this order.</summary>
    public List<OrderItem> Items { get; set; } = new();
}
