using System.Text.Json;
using ModelContextProtocol.Protocol;
using TodoMcpServer.CallToolTypes;
using TodoMcpServer.Services;

namespace TodoMcpServer.Tools;

public class GetToDoListsTool : ToolExecutor
{
    public override string Name { get; } = "GetToDoLists";
    private readonly HttpClient _client;

    public GetToDoListsTool(HttpClient client)
    {
        _client = client;
    }
    
    public override Tool GetToolDefinition()
    {
        return new Tool
        {
            Name = Name,
            Description = "Retrieve all ToDo lists. Only used when no specific list is specified.",
            InputSchema = JsonSerializer.Deserialize<JsonElement>
            ("""
             {
               "type": "object",
               "properties": {},
               "required": []
             }
             """)
        };
    }
    
    protected override string GetSuccessMessage() => "All lists were retrieved successfully.";
    protected override string GetFailureMessagePrefix() => "There was an error retrieving the lists.";
    
    protected override async ValueTask<HttpResponseMessage> Logic(CallToolRequest request, CancellationToken cancellationToken)
    {
        return await _client.GetAsync($"http://localhost:5083/api/todolists", cancellationToken);
    }
}