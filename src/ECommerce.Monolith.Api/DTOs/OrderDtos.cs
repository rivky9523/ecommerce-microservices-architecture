using System.ComponentModel.DataAnnotations;

namespace ECommerce.Monolith.Api.DTOs;

/// <summary>A single requested line in a new order.</summary>
public record CreateOrderItemRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; init; }

    [Range(1, 10_000)]
    public int Quantity { get; init; }
}

/// <summary>Payload for placing an order (POST /api/orders).</summary>
public record CreateOrderRequest
{
    [Required, EmailAddress, MaxLength(256)]
    public string CustomerEmail { get; init; } = string.Empty;

    [Required, MinLength(1)]
    public List<CreateOrderItemRequest> Items { get; init; } = new();
}

/// <summary>Shape of a single order line returned to clients.</summary>
public record OrderItemResponse
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
}

/// <summary>Shape of an order returned to clients.</summary>
public record OrderResponse
{
    public int Id { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public decimal TotalAmount { get; init; }
    public List<OrderItemResponse> Items { get; init; } = new();
}
