using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using TodoMcpServer.CallToolTypes;
using TodoMcpServer.Services;

namespace TodoMcpServer.Configuration;

public class McpServerOptionsFactory
{
    public McpServerOptions Create(IEnumerable<ToolExecutor> tools)
    {
        return new McpServerOptions
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
                        var toolRequest = new CallToolRequest
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
    }
}