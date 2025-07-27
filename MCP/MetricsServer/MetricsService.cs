using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

namespace MetricsMCP;

public class MetricsService
{
        
    public object GeneratePaymentsServiceMetrics(string timeRange)
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

    // Stub for GenerateOrdersServiceMetrics to avoid similar errors
    public object GenerateOrdersServiceMetrics(string timeRange)
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

    // Stub for GenerateAlerts to avoid similar errors
    public object[] GenerateAlerts(string? filterService)
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
}


