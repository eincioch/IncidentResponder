using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

namespace LogQueryMCP;

public class LogQueryService
{
    public static string ExtractTimestamp(string logLine)
    {
        var parts = logLine.Split(' ', 3);
        if (parts.Length >= 2)
            return $"{parts[0]} {parts[1]}";
        return "Unknown";
    }

    // Helper method to extract log level from log line
    public static string ExtractLogLevel(string logLine)
    {
        if (logLine.Contains("[ERROR]")) return "ERROR";
        if (logLine.Contains("[WARN]")) return "WARN";
        if (logLine.Contains("[INFO]")) return "INFO";
        if (logLine.Contains("[DEBUG]")) return "DEBUG";
        return "UNKNOWN";
    }
}