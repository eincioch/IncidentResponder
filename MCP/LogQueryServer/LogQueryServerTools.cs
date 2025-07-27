using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using ModelContextProtocol.Server;

namespace LogQueryMCP;

[McpServerToolType]
public class LogQueryServerTools
{
    private readonly LogQueryService logQueryService;

    public LogQueryServerTools(LogQueryService logQueryService)
    {
        this.logQueryService = logQueryService;
    }

    [McpServerTool, Description("Search through service logs for specific terms or patterns")]
    public async Task<string> QueryLogs(string service, string search_term, int lines = 20)
    {
        try
        {
            var maxLines = Math.Min(Math.Max(lines, 1), 100);

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
                .Where(line => line.Contains(search_term, StringComparison.OrdinalIgnoreCase))
                .Take(maxLines)
                .Select((line, index) => new
                {
                    line_number = Array.IndexOf(allLines, line) + 1,
                    timestamp = LogQueryService.ExtractTimestamp(line),
                    level = LogQueryService.ExtractLogLevel(line),
                    content = line,
                    highlighted_term = search_term
                })
                .ToArray();

            var result = new
            {
                status = "success",
                service = service,
                search_term = search_term,
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
    }

    [McpServerTool, Description("Get the most recent log entries for a service")]
    public async Task<string> GetRecentLogs(string service, int count = 10)
    {
        try
        {
            count = Math.Min(Math.Max(count, 1), 50);

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
                    timestamp = LogQueryService.ExtractTimestamp(line),
                    level = LogQueryService.ExtractLogLevel(line),
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
    }
}