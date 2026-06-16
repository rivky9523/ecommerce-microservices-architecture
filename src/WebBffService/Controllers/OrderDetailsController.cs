using Microsoft.AspNetCore.Mvc;
using WebBffService.Clients;
using WebBffService.DTOs;

namespace WebBffService.Controllers;

/// <summary>
/// The BFF aggregation endpoint. "Order details" is not a single backend
/// resource — it combines the order (OrderService) with each item's current
/// product details (ProductCatalogService) into one client-shaped response.
/// This aggregation logic is client-specific and intentionally lives here,
/// NOT in the gateway.
/// </summary>
[ApiController]
[Route("api/order-details")]
public class OrderDetailsController : ControllerBase
{
    private readonly OrderClient _orders;
    private readonly ProductCatalogClient _catalog;

    public OrderDetailsController(OrderClient orders, ProductCatalogClient catalog)
    {
        _orders = orders;
        _catalog = catalog;
    }

    [HttpGet("{orderId:int}")]
    public async Task<ActionResult<OrderDetailsResponse>> Get(int orderId)
    {
        // 1. Get the order from OrderService.
        var order = await _orders.GetOrderAsync(orderId);
        if (order is null)
            return NotFound(new { error = $"Order {orderId} was not found." });

        // 2. For each line, fetch live product details from ProductCatalogService.
        var lines = new List<OrderDetailLine>();
        foreach (var item in order.Items)
        {
            var product = await _catalog.GetProductAsync(item.ProductId);
            lines.Add(new OrderDetailLine
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPriceAtOrder = item.UnitPrice,
                ProductNameAtOrder = item.ProductName,
                LineTotal = item.UnitPrice * item.Quantity,
                Product = product is null ? null : new ProductInfo
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    CurrentPrice = product.Price,
                    Category = product.Category,
                    IsActive = product.IsActive,
                    Attributes = product.Attributes
                }
            });
        }

        // 3. Return the combined, client-shaped response.
        return Ok(new OrderDetailsResponse
        {
            OrderId = order.Id,
            CustomerEmail = order.CustomerEmail,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            TotalAmount = order.TotalAmount,
            RejectionReason = order.RejectionReason,
            Items = lines
        });
    }
}
