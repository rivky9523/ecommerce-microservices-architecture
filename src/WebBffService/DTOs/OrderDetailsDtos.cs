namespace WebBffService.DTOs;

// --- Shapes used to deserialize responses from the backend services ---

/// <summary>Order as returned by OrderService.</summary>
public record BackendOrder(
    int Id,
    string CustomerEmail,
    string Status,
    DateTime CreatedAt,
    decimal TotalAmount,
    string? RejectionReason,
    List<BackendOrderItem> Items);

public record BackendOrderItem(
    string ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity);

/// <summary>Product as returned by ProductCatalogService.</summary>
public record BackendProduct(
    string Id,
    string Name,
    string Description,
    decimal Price,
    string Category,
    bool IsActive,
    Dictionary<string, string> Attributes);

// --- The aggregated shape returned to the web client ---

/// <summary>Current catalog details for a product (may be null if it no longer exists).</summary>
public record ProductInfo
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal CurrentPrice { get; init; }
    public string Category { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public Dictionary<string, string> Attributes { get; init; } = new();
}

/// <summary>One order line enriched with live product details.</summary>
public record OrderDetailLine
{
    public string ProductId { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPriceAtOrder { get; init; }
    public string ProductNameAtOrder { get; init; } = string.Empty;
    public decimal LineTotal { get; init; }
    public ProductInfo? Product { get; init; }
}

/// <summary>The combined order-details response shaped for a web client.</summary>
public record OrderDetailsResponse
{
    public int OrderId { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public decimal TotalAmount { get; init; }
    public string? RejectionReason { get; init; }
    public List<OrderDetailLine> Items { get; init; } = new();
}
