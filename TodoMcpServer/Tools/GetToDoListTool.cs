using System.Text.Json;
using ModelContextProtocol.Protocol;
using TodoMcpServer.CallToolTypes;
using TodoMcpServer.Services;

namespace TodoMcpServer.Tools;

public class GetToDoListTool : ToolExecutor
{
    public override string Name { get; } = "GetToDoList";
    private readonly HttpClient _client;

    public GetToDoListTool(HttpClient client)
    {
        _client = client;
    }
    
    public override Tool GetToolDefinition()
    {
        return new Tool
        {
            Name = Name,
            Description = "Retrieve one list by its name when an specific list is specified.",
            InputSchema = JsonSerializer.Deserialize<JsonElement>
            ("""
             {
               "type": "object",
               "properties": {
                  "name": {
                      "type": "string",
                      "description": "The name of the list."
                  }
               },
               "required": []
             }
             """)
        };
    }
    
    protected override string GetSuccessMessage() => "The list was retrieved successfully.";
    protected override string GetFailureMessagePrefix() => "There was an error retrieving the lists.";
    
    protected override async ValueTask<HttpResponseMessage> Logic(CallToolRequest request, CancellationToken cancellationToken)
    {
        var name = request.Arguments?["name"];
        var response =  await _client.GetAsync($"http://localhost:5083/api/todolists?name={name}", cancellationToken);
        await ValidateResponseIsNotEmpty(response, cancellationToken);
        return response;
    }

    private async Task ValidateResponseIsNotEmpty(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
    
            if (jsonContent.Trim() == "[]")
            {
                throw new ArgumentException("There are no lists with the specified name.");
            }
        }
    }
}