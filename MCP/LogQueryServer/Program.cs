using LogQueryMCP;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

builder.Services.
    AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<LogQueryServerTools>();

builder.Services.AddSingleton<LogQueryService>();

await builder.Build().RunAsync();