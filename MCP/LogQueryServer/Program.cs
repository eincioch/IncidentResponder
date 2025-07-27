using ModelContextProtocol;
using Microsoft.Extensions.Logging;
using System.Text.Json;

// MCP Log Query Server - helps GitHub Copilot Agent analyze production logs
// This server exposes log querying capabilities to assist in incident response

var builder = ModelContextProtocol.McpServerBuilder.CreateBuilder(args);

// Configure logging for the MCP server itself
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

var app = builder.Build();

// Tool: query_logs - Search through log files for specific terms
// GitHub Copilot Agent can use this to investigate errors mentioned in PaymentsProcessor
app.AddTool("query_logs", "Search through service logs for specific terms or patterns", new
{
    type = "object",
    properties = new
    {
        service = new
        {
            type = "string",
            description = "The service name to search logs for (e.g., PaymentsService, OrdersService)",
            @enum = new[] { "PaymentsService", "OrdersService" }
        },
        search_term = new
        {
            type = "string",
            description = "The term to search for in logs (e.g., 'error', 'exception', 'NullReference')"
        },
        lines = new
        {
            type = "integer",
            description = "Number of log lines to return (default: 20)",
            minimum = 1,
            maximum = 100
        }
    },
    required = new[] { "service", "search_term" }
}, async (arguments) =>
{
    try
    {
        var service = arguments["service"]?.ToString() ?? "";
        var searchTerm = arguments["search_term"]?.ToString() ?? "";
        var maxLines = 20;
        int parsedLines = 0;

        if (arguments.ContainsKey("lines") && int.TryParse(arguments["lines"]?.ToString(), out parsedLines))
        {
            maxLines = Math.Min(Math.Max(parsedLines, 1), 100);
        }

        // Map service name to log file
        var logFile = service.ToLower() switch
        {
            "paymentsservice" => "payments.log",
            "ordersservice" => "orders.log",
            _ => throw new ArgumentException($"Unknown service: {service}")
        };

        var logPath = Path.Combine(Environment.GetEnvironmentVariable("LOG_PATH") ?? "../../Logs", logFile);

        if (!File.Exists(logPath))
        {
            return JsonSerializer.Serialize(new
            {
                status = "error",
                message = $"Log file not found: {logPath}",
                results = Array.Empty<object>()
            });
        }

        var allLines = await File.ReadAllLinesAsync(logPath);
        var matchingLines = allLines
            .Where(line => line.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .Take(maxLines)
            .Select((line, index) => new
            {
                line_number = Array.IndexOf(allLines, line) + 1,
                timestamp = ExtractTimestamp(line),
                level = ExtractLogLevel(line),
                content = line,
                highlighted_term = searchTerm
            })
            .ToArray();

        var result = new
        {
            status = "success",
            service = service,
            search_term = searchTerm,
            total_matches = matchingLines.Length,
            log_file = logFile,
            results = matchingLines
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }
    catch (Exception ex)
    {
        return JsonSerializer.Serialize(new
        {
            status = "error",
            message = ex.Message,
            results = Array.Empty<object>()
        });
    }
});

// Tool: get_recent_logs - Get the most recent log entries for a service
// Useful for GitHub Copilot Agent to see the latest activity
app.AddTool("get_recent_logs", "Get the most recent log entries for a service", new
{
    type = "object",
    properties = new
    {
        service = new
        {
            type = "string",
            description = "The service name to get logs for",
            @enum = new[] { "PaymentsService", "OrdersService" }
        },
        count = new
        {
            type = "integer",
            description = "Number of recent log lines to return (default: 10)",
            minimum = 1,
            maximum = 50
        }
    },
    required = new[] { "service" }
}, async (arguments) =>
{
    try
    {
        var service = arguments["service"]?.ToString() ?? "";
        var count = 10;

        if (arguments.ContainsKey("count") && int.TryParse(arguments["count"]?.ToString(), out int parsedCount))
        {
            count = Math.Min(Math.Max(parsedCount, 1), 50);
        }

        var logFile = service.ToLower() switch
        {
            "paymentsservice" => "payments.log",
            "ordersservice" => "orders.log",
            _ => throw new ArgumentException($"Unknown service: {service}")
        };

        var logPath = Path.Combine(Environment.GetEnvironmentVariable("LOG_PATH") ?? "../../Logs", logFile);

        if (!File.Exists(logPath))
        {
            return JsonSerializer.Serialize(new
            {
                status = "error",
                message = $"Log file not found: {logPath}",
                results = Array.Empty<object>()
            });
        }

        var allLines = await File.ReadAllLinesAsync(logPath);
        var recentLines = allLines
            .TakeLast(count)
            .Select((line, index) => new
            {
                line_number = allLines.Length - count + index + 1,
                timestamp = ExtractTimestamp(line),
                level = ExtractLogLevel(line),
                content = line
            })
            .ToArray();

        var result = new
        {
            status = "success",
            service = service,
            total_lines = allLines.Length,
            returned_lines = recentLines.Length,
            log_file = logFile,
            results = recentLines
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }
    catch (Exception ex)
    {
        return JsonSerializer.Serialize(new
        {
            status = "error",
            message = ex.Message,
            results = Array.Empty<object>()
        });
    }
});

// Helper method to extract timestamp from log line
static string ExtractTimestamp(string logLine)
{
    var parts = logLine.Split(' ', 3);
    if (parts.Length >= 2)
        return $"{parts[0]} {parts[1]}";
    return "Unknown";
}

// Helper method to extract log level from log line
static string ExtractLogLevel(string logLine)
{
    if (logLine.Contains("[ERROR]")) return "ERROR";
    if (logLine.Contains("[WARN]")) return "WARN";
    if (logLine.Contains("[INFO]")) return "INFO";
    if (logLine.Contains("[DEBUG]")) return "DEBUG";
    return "UNKNOWN";
}

await app.RunAsync();