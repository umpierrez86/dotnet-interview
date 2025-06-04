using System.Text;
using System.Text.Json;
using ModelContextProtocol.Protocol;
using TodoMcpServer.CallToolTypes;
using TodoMcpServer.DataExtractors;
using TodoMcpServer.Inputs;
using TodoMcpServer.InputValidator;
using TodoMcpServer.Interfaces;
using TodoMcpServer.Services;

namespace TodoMcpServer.Tools;

public class UpdateToDoListTool : ToolExecutor
{
    public override string Name { get; } = "UpdateToDoList";
    private readonly UpdateToDoArgumentValidator _argsValidator = new();
    private readonly UpdateToDoValidator _objectValidator = new();
    private readonly HttpClient _client;
    private readonly ITodoLookupService _lookupService;

    public UpdateToDoListTool(HttpClient client, ITodoLookupService lookupService)
    {
        _client = client;
        _lookupService = lookupService;
    }

    public override Tool GetToolDefinition()
    {
        return new Tool
        {
            Name = Name,
            Description = "Creates a new ToDo list. Use this when the list exits and the data needs to be updated.",
            InputSchema = JsonSerializer.Deserialize<JsonElement>
            ("""
             {
             "type": "object",
                 "properties": {
                     "name": {
                         "type": "string",
                         "description": "The name of the ToDo list"
                     },
                     "newName": {
                          "type": "string",
                          "description": "New name for the ToDo list"
                     }
                 },
                 "required": ["name", "newName"]
             }
             """)
        };
    }

    protected override string GetSuccessMessage() => "Update of ToDo list was successful";
    protected override string GetFailureMessagePrefix() => "Update of ToDo list failed";
    
    protected override async ValueTask<HttpResponseMessage> Logic(CallToolRequest request, CancellationToken cancellationToken)
    {
        var toDoList = await ExtractToDoListAsync(request.Arguments, cancellationToken);
        return await UpdateToDoListAsync(toDoList, cancellationToken);
    }
    
    private async ValueTask<UpdateToDoList> ExtractToDoListAsync(IReadOnlyDictionary<string, JsonElement> arguments, CancellationToken cancellationToken)
    {
        var extractor = new RequestExtractor<UpdateToDoList>(
            _argsValidator,
            _objectValidator,
            args => new UpdateToDoList
            {
                Name = args["name"].GetString()!,
                NewName = args["newName"].GetString()!,
            }
        );
        return await extractor.ExtractAsync(arguments, cancellationToken);
    }

    private async ValueTask<HttpResponseMessage> UpdateToDoListAsync(UpdateToDoList toDoList, CancellationToken cancellationToken)
    {
        var listId = await _lookupService.GetListIdByNameAsync(toDoList.Name, cancellationToken);
        
        var payload = new
        {
            Name = toDoList.NewName,
        };

        var putContent = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        return await _client.PutAsync($"http://localhost:5083/api/todoLists/{listId}", 
            putContent, cancellationToken);
    }
}