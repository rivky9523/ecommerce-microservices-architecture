using ECommerce.Monolith.Api.DTOs;
using ECommerce.Monolith.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Monolith.Api.Controllers;

/// <summary>HTTP endpoints for placing and reading orders.</summary>
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orders;

    public OrdersController(OrderService orders) => _orders = orders;

    /// <summary>
    /// Place an order. Returns 201 with the confirmed order on success,
    /// or 422 with a clear message when products are missing/inactive or
    /// inventory is insufficient.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Place(CreateOrderRequest request)
    {
        var result = await _orders.PlaceOrderAsync(request);
        if (result.Succeeded)
            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);

        // A failed business rule (bad product / not enough stock) is a 422.
        return UnprocessableEntity(new { error = result.Error });
    }

    /// <summary>List all orders (newest first).</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetAll()
        => Ok(await _orders.GetAllAsync());

    /// <summary>Get a single order by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponse>> GetById(int id)
    {
        var result = await _orders.GetByIdAsync(id);
        return result.Succeeded ? Ok(result.Value) : NotFound(new { error = result.Error });
    }
}
