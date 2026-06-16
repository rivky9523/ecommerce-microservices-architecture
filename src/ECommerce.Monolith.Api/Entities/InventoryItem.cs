namespace ECommerce.Monolith.Api.Entities;

/// <summary>
/// Stock record for a single product.
/// Phase 1 keeps it simple: available stock is what can still be sold,
/// reserved stock is what has already been committed to confirmed orders.
/// </summary>
public class InventoryItem
{
    public int Id { get; set; }

    /// <summary>Foreign key to the owning <see cref="Product"/>.</summary>
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    /// <summary>Units that can still be sold.</summary>
    public int QuantityAvailable { get; set; }

    /// <summary>Units already committed to confirmed orders.</summary>
    public int QuantityReserved { get; set; }
}
