using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using AttendanceSystem.API.Data;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.API.Services;

public record WebhookEvent(Guid OrganizationId, string EventType, object Payload);

public class WebhookService : BackgroundService
{
    private readonly Channel<WebhookEvent> _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WebhookService> _logger;
    private readonly HttpClient _httpClient;

    public WebhookService(IServiceScopeFactory scopeFactory, ILogger<WebhookService> logger)
    {
        _channel = Channel.CreateBounded<WebhookEvent>(100);
        _scopeFactory = scopeFactory;
        _logger = logger;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    }

    public ChannelWriter<WebhookEvent> Writer => _channel.Writer;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var evt in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessEvent(evt, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process webhook event {EventType}", evt.EventType);
            }
        }
    }

    private async Task ProcessEvent(WebhookEvent evt, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var endpoints = await db.Set<Models.WebhookEndpoint>()
            .Where(w => w.OrganizationId == evt.OrganizationId && w.IsActive)
            .ToListAsync(ct);

        var json = JsonSerializer.Serialize(new
        {
            @event = evt.EventType,
            timestamp = DateTime.UtcNow,
            data = evt.Payload
        });

        foreach (var endpoint in endpoints)
        {
            if (!endpoint.Events.Split(',').Contains(evt.EventType))
                continue;

            try
            {
                var signature = ComputeHmac(json, endpoint.Secret);
                var request = new HttpRequestMessage(HttpMethod.Post, endpoint.Url);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                request.Headers.Add("X-Webhook-Signature", signature);
                request.Headers.Add("X-Webhook-Event", evt.EventType);

                await _httpClient.SendAsync(request, ct);

                endpoint.LastTriggeredAt = DateTime.UtcNow;
                await db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deliver webhook to {Url}", endpoint.Url);
            }
        }
    }

    private static string ComputeHmac(string payload, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var hash = HMACSHA256.HashData(keyBytes, payloadBytes);
        return $"sha256={Convert.ToHexStringLower(hash)}";
    }
}
