using McpServer;
using Microsoft.Extensions.Logging;
using System.Text.Json;

// MCP Metrics Server - provides system metrics to help GitHub Copilot Agent understand service health
// This server exposes performance and error metrics to assist in incident analysis

var builder = McpServerBuilder.CreateBuilder(args);

// Configure logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

var app = builder.Build();

// Tool: get_metrics - Retrieve system metrics for a specific service
// GitHub Copilot Agent can use this to understand service health and performance
app.AddTool("get_metrics", "Get current system metrics for a service including CPU, memory, and error rates", new
{
    type = "object",
    properties = new
    {
        service = new
        {
            type = "string",
            description = "The service name to get metrics for",
            @enum = new[] { "PaymentsService", "OrdersService" }
        },
        time_range = new
        {
            type = "string",
            description = "Time range for metrics (default: '1h')",
            @enum = new[] { "5m", "15m", "1h", "6h", "24h" }
        }
    },
    required = new[] { "service" }
}, async (arguments) =>
{
    try
    {
        var service = arguments["service"]?.ToString() ?? "";
        var timeRange = arguments.ContainsKey("time_range") ? arguments["time_range"]?.ToString() ?? "1h" : "1h";

        // Simulate realistic metrics based on the service
        // PaymentsService shows concerning metrics due to the bugs
        // OrdersService shows healthy metrics
        var metrics = service.ToLower() switch
        {
            "paymentsservice" => GeneratePaymentsServiceMetrics(timeRange),
            "ordersservice" => GenerateOrdersServiceMetrics(timeRange),
            _ => throw new ArgumentException($"Unknown service: {service}")
        };

        var result = new
        {
            status = "success",
            service = service,
            time_range = timeRange,
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            metrics = metrics
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }
    catch (Exception ex)
    {
        return JsonSerializer.Serialize(new
        {
            status = "error",
            message = ex.Message
        });
    }
});

// Tool: get_alert_status - Check if there are any active alerts for services
// Helps GitHub Copilot Agent understand the urgency of issues
app.AddTool("get_alert_status", "Get current alert status and active incidents for services", new
{
    type = "object",
    properties = new
    {
        service = new
        {
            type = "string",
            description = "The service name to check alerts for (optional - if not provided, returns all alerts)",
            @enum = new[] { "PaymentsService", "OrdersService" }
        }
    },
    required = Array.Empty<string>()
}, async (arguments) =>
{
    try
    {
        var service = arguments.ContainsKey("service") ? arguments["service"]?.ToString() : null;

        var alerts = GenerateAlerts(service);

        var result = new
        {
            status = "success",
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            total_alerts = alerts.Length,
            critical_alerts = alerts.Count(a => a.severity == "critical"),
            warning_alerts = alerts.Count(a => a.severity == "warning"),
            alerts = alerts
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }
    catch (Exception ex)
    {
        return JsonSerializer.Serialize(new
        {
            status = "error",
            message = ex.Message
        });
    }
});

// Generate metrics for PaymentsService (showing problematic patterns)
static object GeneratePaymentsServiceMetrics(string timeRange)
{
    var random = new Random();
    
    return new
    {
        cpu_usage_percent = Math.Round(45.0 + random.NextDouble() * 20, 2), // High CPU usage
        memory_usage_mb = Math.Round(380.0 + random.NextDouble() * 120, 2), // High memory usage
        memory_usage_percent = Math.Round(76.0 + random.NextDouble() * 15, 2),
        requests_per_minute = Math.Round(150.0 + random.NextDouble() * 50, 2),
        error_rate_percent = Math.Round(12.5 + random.NextDouble() * 7.5, 2), // High error rate!
        response_time_ms = Math.Round(850.0 + random.NextDouble() * 200, 2), // Slow responses
        active_connections = random.Next(25, 45),
        failed_transactions = random.Next(15, 35), // Many failures
        successful_transactions = random.Next(75, 125),
        null_reference_exceptions = random.Next(8, 15), // The bug we planted!
        payment_gateway_timeouts = random.Next(3, 8),
        disk_io_mb_per_sec = Math.Round(2.5 + random.NextDouble() * 1.5, 2),
        network_io_mb_per_sec = Math.Round(1.8 + random.NextDouble() * 0.7, 2),
        garbage_collections_per_minute = random.Next(8, 15),
        thread_pool_usage_percent = Math.Round(65.0 + random.NextDouble() * 25, 2)
    };
}

// Generate metrics for OrdersService (showing healthy patterns)
static object GenerateOrdersServiceMetrics(string timeRange)
{
    var random = new Random();
    
    return new
    {
        cpu_usage_percent = Math.Round(15.0 + random.NextDouble() * 10, 2), // Normal CPU
        memory_usage_mb = Math.Round(180.0 + random.NextDouble() * 40, 2), // Normal memory
        memory_usage_percent = Math.Round(36.0 + random.NextDouble() * 12, 2),
        requests_per_minute = Math.Round(200.0 + random.NextDouble() * 30, 2),
        error_rate_percent = Math.Round(0.5 + random.NextDouble() * 1.0, 2), // Low error rate
        response_time_ms = Math.Round(120.0 + random.NextDouble() * 80, 2), // Fast responses
        active_connections = random.Next(35, 55),
        failed_orders = random.Next(1, 3), // Very few failures
        successful_orders = random.Next(195, 225),
        validation_errors = random.Next(0, 2), // Rare validation issues
        database_query_time_ms = Math.Round(25.0 + random.NextDouble() * 15, 2),
        disk_io_mb_per_sec = Math.Round(1.2 + random.NextDouble() * 0.8, 2),
        network_io_mb_per_sec = Math.Round(0.9 + random.NextDouble() * 0.4, 2),
        garbage_collections_per_minute = random.Next(2, 5),
        thread_pool_usage_percent = Math.Round(25.0 + random.NextDouble() * 15, 2)
    };
}

// Generate alerts based on service health
static object[] GenerateAlerts(string? filterService)
{
    var allAlerts = new[]
    {
        new
        {
            id = "ALERT-001",
            service = "PaymentsService",
            severity = "critical",
            title = "High Error Rate Detected",
            description = "PaymentsService error rate has exceeded 10% threshold (currently 15.2%)",
            triggered_at = DateTime.UtcNow.AddMinutes(-25).ToString("yyyy-MM-ddTHH:mm:ssZ"),
            metric = "error_rate_percent",
            current_value = 15.2,
            threshold = 10.0,
            status = "active"
        },
        new
        {
            id = "ALERT-002", 
            service = "PaymentsService",
            severity = "warning",
            title = "High Response Time",
            description = "PaymentsService average response time above 800ms",
            triggered_at = DateTime.UtcNow.AddMinutes(-18).ToString("yyyy-MM-ddTHH:mm:ssZ"),
            metric = "response_time_ms",
            current_value = 945.3,
            threshold = 800.0,
            status = "active"
        },
        new
        {
            id = "ALERT-003",
            service = "PaymentsService", 
            severity = "critical",
            title = "Frequent NullReferenceExceptions",
            description = "Multiple NullReferenceExceptions detected in PaymentsProcessor.cs",
            triggered_at = DateTime.UtcNow.AddMinutes(-12).ToString("yyyy-MM-ddTHH:mm:ssZ"),
            metric = "null_reference_exceptions",
            current_value = 12.0,
            threshold = 5.0,
            status = "active"
        },
        new
        {
            id = "ALERT-004",
            service = "OrdersService",
            severity = "info",
            title = "All Systems Normal",
            description = "OrdersService operating within normal parameters",
            triggered_at = DateTime.UtcNow.AddMinutes(-60).ToString("yyyy-MM-ddTHH:mm:ssZ"),
            metric = "overall_health",
            current_value = 98.5,
            threshold = 95.0,
            status = "resolved"
        }
    };

    if (string.IsNullOrEmpty(filterService))
        return allAlerts;

    return allAlerts.Where(a => a.service.Equals(filterService, StringComparison.OrdinalIgnoreCase)).ToArray();
}

await app.RunAsync();