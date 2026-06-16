using ECommerce.Monolith.Api.DTOs;
using ECommerce.Monolith.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Monolith.Api.Controllers;

/// <summary>
/// HTTP endpoints for the product catalog. Controllers are intentionally thin:
/// they validate input (via model binding/data annotations) and translate
/// service results into HTTP responses.
/// </summary>
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _products;

    public ProductsController(ProductService products) => _products = products;

    /// <summary>Create a new product (also creates an empty inventory record).</summary>
    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request)
    {
        var created = await _products.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>List all products.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAll()
        => Ok(await _products.GetAllAsync());

    /// <summary>Get a single product by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponse>> GetById(int id)
    {
        var result = await _products.GetByIdAsync(id);
        return result.Succeeded ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    /// <summary>Update an existing product.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductResponse>> Update(int id, UpdateProductRequest request)
    {
        var result = await _products.UpdateAsync(id, request);
        return result.Succeeded ? Ok(result.Value) : NotFound(new { error = result.Error });
    }
}
