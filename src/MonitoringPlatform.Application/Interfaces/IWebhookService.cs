namespace MonitoringPlatform.Application.Interfaces;

public interface IWebhookService
{
    Task SendWebhookAsync(string webhookUrl, object payload);
}
