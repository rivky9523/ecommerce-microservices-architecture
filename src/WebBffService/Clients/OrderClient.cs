using System.Net;
using WebBffService.DTOs;

namespace WebBffService.Clients;

/// <summary>Typed HTTP client the BFF uses to read orders from OrderService.</summary>
public class OrderClient
{
    private readonly HttpClient _http;

    public OrderClient(HttpClient http) => _http = http;

    /// <summary>Returns the order, or null if it does not exist (404).</summary>
    public async Task<BackendOrder?> GetOrderAsync(int orderId)
    {
        var response = await _http.GetAsync($"/api/orders/{orderId}");
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BackendOrder>();
    }
}
