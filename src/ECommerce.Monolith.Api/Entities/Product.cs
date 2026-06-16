namespace ECommerce.Monolith.Api.Entities;

/// <summary>
/// A sellable item in the catalog. This is the persisted database entity
/// (not the shape returned to API clients — that is handled by DTOs).
/// </summary>
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;

    /// <summary>Soft on/off switch so products can be hidden without deletion.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>One-to-one inventory record for this product.</summary>
    public InventoryItem? Inventory { get; set; }
}
