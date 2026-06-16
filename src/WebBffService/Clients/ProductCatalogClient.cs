using System.Net;
using WebBffService.DTOs;

namespace WebBffService.Clients;

/// <summary>
/// Typed HTTP client the BFF uses to read product details. Its base address
/// points at the catalog load balancer, so BFF reads are load-balanced too.
/// </summary>
public class ProductCatalogClient
{
    private readonly HttpClient _http;

    public ProductCatalogClient(HttpClient http) => _http = http;

    /// <summary>Returns the product, or null if it does not exist (404).</summary>
    public async Task<BackendProduct?> GetProductAsync(string productId)
    {
        var response = await _http.GetAsync($"/api/products/{productId}");
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BackendProduct>();
    }
}
