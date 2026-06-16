using System.ComponentModel.DataAnnotations;

namespace ECommerce.Monolith.Api.DTOs;

/// <summary>
/// Payload to set/update inventory for a product (PUT /api/inventory/{productId}).
/// We set absolute quantities rather than deltas to keep Phase 1 simple.
/// </summary>
public record UpdateInventoryRequest
{
    [Range(0, 1_000_000)]
    public int QuantityAvailable { get; init; }

    [Range(0, 1_000_000)]
    public int QuantityReserved { get; init; }
}

/// <summary>Shape returned to clients for an inventory record.</summary>
public record InventoryResponse
{
    public int ProductId { get; init; }
    public int QuantityAvailable { get; init; }
    public int QuantityReserved { get; init; }
}
