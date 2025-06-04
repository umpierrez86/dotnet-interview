using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using TodoMcpServer.CallToolTypes;
using TodoMcpServer.Interfaces;
using TodoMcpServer.Services;
using TodoMcpServer.Tools;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddScoped<ITodoLookupService, TodoLookupService>();

builder.Services.AddHttpClient<CreateToDoListTool>();
builder.Services.AddHttpClient<GetToDoListsTool>();
builder.Services.AddHttpClient<GetToDoListTool>();
builder.Services.AddHttpClient<UpdateToDoListTool>();
builder.Services.AddHttpClient<DeleteToDoListTool>();
builder.Services.AddHttpClient<CreateItemTool>();
builder.Services.AddHttpClient<UpdateItemTool>();
builder.Services.AddHttpClient<DeleteItemTool>();
builder.Services.AddHttpClient<CompleteItemTool>();

builder.Services.AddTransient<ToolExecutor, CreateToDoListTool>();
builder.Services.AddTransient<ToolExecutor, GetToDoListsTool>();
builder.Services.AddTransient<ToolExecutor, GetToDoListTool>();
builder.Services.AddTransient<ToolExecutor, UpdateToDoListTool>();
builder.Services.AddTransient<ToolExecutor, DeleteToDoListTool>();
builder.Services.AddTransient<ToolExecutor, CreateItemTool>();
builder.Services.AddTransient<ToolExecutor, UpdateItemTool>();
builder.Services.AddTransient<ToolExecutor, DeleteItemTool>();
builder.Services.AddTransient<ToolExecutor, CompleteItemTool>();

var host = builder.Build();

await using var scope = host.Services.CreateAsyncScope();
var tools = scope.ServiceProvider.GetServices<ToolExecutor>().ToList();



McpServerOptions options = new()
{
    ServerInfo = new Implementation { Name = "MyMcpServer", Version = "1.0.0" },
    Capabilities = new ServerCapabilities
    {
        Tools = new ToolsCapability
        {
            ListToolsHandler = (_, _) =>
                ValueTask.FromResult(new ListToolsResult
                {
                    Tools = tools.Select(t => t.GetToolDefinition()).ToList()
                }),

            CallToolHandler = async (request, cancellationToken) =>
            {
                CallToolRequest toolRequest = new CallToolRequest
                {
                    Name = request.Params?.Name,
                    Arguments = request.Params?.Arguments
                };
                
                var tool = tools.FirstOrDefault(t => t.Name == toolRequest.Name);
                if (tool is null)
                {
                    throw new McpException($"Unknown tool: '{request.Params?.Name}'");
                }
                
                return await tool.HandleAsync(toolRequest, cancellationToken);
            }
        }
    }
};

await using IMcpServer server = McpServerFactory.Create(new StdioServerTransport("MyMcpServer"), options);
await server.RunAsync();