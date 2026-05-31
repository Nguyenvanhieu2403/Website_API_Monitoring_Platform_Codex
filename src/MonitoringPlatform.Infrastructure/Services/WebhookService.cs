using MonitoringPlatform.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MonitoringPlatform.Infrastructure.Services;

public class WebhookService : IWebhookService
{
    private readonly ILogger<WebhookService> _logger;
    private readonly HttpClient _httpClient;

    public WebhookService(ILogger<WebhookService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task SendWebhookAsync(string webhookUrl, object payload)
    {
        try
        {
            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(webhookUrl, content);
            response.EnsureSuccessStatusCode(); // Throws an exception if the HTTP response status code is not 2xx

            _logger.LogInformation("Webhook sent successfully to {WebhookUrl}", webhookUrl);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while sending webhook to {WebhookUrl}: {Message}", webhookUrl, ex.Message);
            throw; // Re-throw to be handled by notification retry logic
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send webhook to {WebhookUrl}: {Message}", webhookUrl, ex.Message);
            throw; // Re-throw to be handled by notification retry logic
        }
    }
}
