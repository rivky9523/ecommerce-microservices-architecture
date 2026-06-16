using System.ComponentModel.DataAnnotations;

namespace ECommerce.Monolith.Api.DTOs;

/// <summary>Payload for creating a product (POST /api/products).</summary>
public record CreateProductRequest
{
    [Required, MaxLength(200)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; init; } = string.Empty;

    [Range(0.01, 1_000_000)]
    public decimal Price { get; init; }

    [MaxLength(100)]
    public string Category { get; init; } = string.Empty;

    public bool IsActive { get; init; } = true;
}

/// <summary>Payload for updating a product (PUT /api/products/{id}).</summary>
public record UpdateProductRequest
{
    [Required, MaxLength(200)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; init; } = string.Empty;

    [Range(0.01, 1_000_000)]
    public decimal Price { get; init; }

    [MaxLength(100)]
    public string Category { get; init; } = string.Empty;

    public bool IsActive { get; init; } = true;
}

/// <summary>Shape returned to clients for a product.</summary>
public record ProductResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Category { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}
