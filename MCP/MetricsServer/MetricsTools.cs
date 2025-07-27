using System;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq; // Add this for .Count extension method
using ModelContextProtocol.Server;

namespace MetricsMCP;

[McpServerToolType]
public class MetricsTools
{
    private readonly MetricsService metricsService;

    public MetricsTools(MetricsService metricsService)
    {
        this.metricsService = metricsService;
    }

    [McpServerTool, Description("Retrieve system metrics for a service")]
    public async Task<string> GetMetrics(string service = "PaymentsService", string time_range = "1h")
    {
        var metrics = service.ToLower() switch
        {
            "paymentsservice" => metricsService.GeneratePaymentsServiceMetrics(time_range),
            "ordersservice" => metricsService.GenerateOrdersServiceMetrics(time_range),
            _ => throw new ArgumentException($"Unknown service: {service}")
        };

        return JsonSerializer.Serialize(new
        {
            status = "success",
            service,
            time_range,
            timestamp = DateTime.UtcNow.ToString("o"),
            metrics
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Get current alert status for services")]
    public async Task<string> GetAlertStatus(string? service = null)
    {
        var alerts = metricsService.GenerateAlerts(service);

        // Cast alerts to dynamic to access severity property
        return JsonSerializer.Serialize(new
        {
            status = "success",
            timestamp = DateTime.UtcNow.ToString("o"),
            total_alerts = alerts.Length,
            critical_alerts = alerts.Count(a => ((dynamic)a).severity == "critical"),
            warning_alerts = alerts.Count(a => ((dynamic)a).severity == "warning"),
            alerts
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}

