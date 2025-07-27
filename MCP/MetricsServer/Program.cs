using MetricsMCP;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

builder.Services.
    AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<MetricsTools>();

builder.Services.AddSingleton<MetricsService>();

await builder.Build().RunAsync();