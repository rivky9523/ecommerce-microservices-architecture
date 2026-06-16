namespace ECommerce.Monolith.Api.Entities;

/// <summary>
/// A single line in an order. ProductName and UnitPrice are copied
/// (snapshotted) at order time so the order stays correct even if the
/// product is later renamed or repriced.
/// </summary>
public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
