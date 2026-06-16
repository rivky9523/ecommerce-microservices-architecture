using ECommerce.Monolith.Api.DTOs;
using ECommerce.Monolith.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Monolith.Api.Controllers;

/// <summary>HTTP endpoints for reading and updating product inventory.</summary>
[ApiController]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly InventoryService _inventory;

    public InventoryController(InventoryService inventory) => _inventory = inventory;

    /// <summary>Get the current stock for a product.</summary>
    [HttpGet("{productId:int}")]
    public async Task<ActionResult<InventoryResponse>> Get(int productId)
    {
        var result = await _inventory.GetByProductIdAsync(productId);
        return result.Succeeded ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    /// <summary>Set/update the available and reserved quantities for a product.</summary>
    [HttpPut("{productId:int}")]
    public async Task<ActionResult<InventoryResponse>> Update(int productId, UpdateInventoryRequest request)
    {
        var result = await _inventory.UpdateAsync(productId, request);
        return result.Succeeded ? Ok(result.Value) : NotFound(new { error = result.Error });
    }
}
