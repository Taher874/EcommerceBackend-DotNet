using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EcommerceBackend.Configurations;
using EcommerceBackend.DTOs.Payment;
using EcommerceBackend.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace EcommerceBackend.Services.Implementations;

public class PayPalService : IPayPalService
{
    private readonly HttpClient _httpClient;
    private readonly PayPalSettings _settings;

    public PayPalService(
        HttpClient httpClient,
        IOptions<PayPalSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    private async Task<string> GetAccessTokenAsync()
    {
        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes(
                $"{_settings.ClientId}:{_settings.ClientSecret}"
            )
        );

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_settings.BaseUrl}/v1/oauth2/token"
        );

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Basic",
                credentials
            );

        request.Content = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials"
            }
        );

        var response = await _httpClient.SendAsync(request);

        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"PayPal authentication failed: {content}"
            );
        }

        using var json = JsonDocument.Parse(content);

        return json.RootElement
            .GetProperty("access_token")
            .GetString()!;
    }

    public async Task<PayPalCreateOrderResult> CreateOrderAsync(
        decimal amount,
        string orderNumber)
    {
        var accessToken = await GetAccessTokenAsync();

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_settings.BaseUrl}/v2/checkout/orders"
        );

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken
            );

        request.Headers.Add(
            "Prefer",
            "return=representation"
        );

        var body = new
        {
            intent = "CAPTURE",

            purchase_units = new[]
            {
                new
                {
                    reference_id = orderNumber,

                    amount = new
                    {
                        currency_code = "USD",
                        value = amount.ToString("0.00")
                    }
                }
            },

            application_context = new
            {
                brand_name = "Ecommerce Store",

                user_action = "PAY_NOW",

                return_url = _settings.ReturnUrl,

                cancel_url = _settings.CancelUrl
            }
        };

        request.Content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.SendAsync(request);

        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"PayPal order creation failed: {content}"
            );
        }

        using var json = JsonDocument.Parse(content);

        var root = json.RootElement;

        var paypalOrderId = root
            .GetProperty("id")
            .GetString()!;

        string? approvalUrl = null;

        if (root.TryGetProperty("links", out var links))
        {
            foreach (var link in links.EnumerateArray())
            {
                var rel = link
                    .GetProperty("rel")
                    .GetString();

                if (rel == "approve" || rel == "payer-action")
                {
                    approvalUrl = link
                        .GetProperty("href")
                        .GetString();

                    break;
                }
            }
        }

        if (string.IsNullOrWhiteSpace(approvalUrl))
        {
            throw new Exception(
                "PayPal approval URL was not returned."
            );
        }

        return new PayPalCreateOrderResult
        {
            OrderId = paypalOrderId,

            ApprovalUrl = approvalUrl
        };
    }

    public async Task<PayPalCaptureResult> CaptureOrderAsync(
    string paypalOrderId)
{
    var accessToken = await GetAccessTokenAsync();

    using var request = new HttpRequestMessage(
        HttpMethod.Post,
        $"{_settings.BaseUrl}/v2/checkout/orders/{paypalOrderId}/capture"
    );

    request.Headers.Authorization =
        new AuthenticationHeaderValue(
            "Bearer",
            accessToken
        );

    request.Headers.Add(
        "Prefer",
        "return=representation"
    );

    request.Content = new StringContent(
        "{}",
        Encoding.UTF8,
        "application/json"
    );

    var response = await _httpClient.SendAsync(request);

    var content = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
    {
        throw new Exception(
            $"PayPal capture failed: {content}"
        );
    }

    using var json = JsonDocument.Parse(content);

    var root = json.RootElement;

    var orderId = root
        .GetProperty("id")
        .GetString()!;

    var status = root
        .GetProperty("status")
        .GetString()!;

    string? captureId = null;

    if (root.TryGetProperty("purchase_units", out var purchaseUnits))
    {
        var firstUnit = purchaseUnits
            .EnumerateArray()
            .FirstOrDefault();

        if (firstUnit.TryGetProperty("payments", out var payments))
        {
            if (payments.TryGetProperty("captures", out var captures))
            {
                var firstCapture = captures
                    .EnumerateArray()
                    .FirstOrDefault();

                if (firstCapture.TryGetProperty("id", out var id))
                {
                    captureId = id.GetString();
                }
            }
        }
    }

    return new PayPalCaptureResult
    {
        Success = status == "COMPLETED",

        OrderId = orderId,

        CaptureId = captureId,

        Status = status
    };
}
}