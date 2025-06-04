using System.Text.Json;
using ModelContextProtocol.Protocol;
using TodoMcpServer.CallToolTypes;
using TodoMcpServer.DataExtractors;
using TodoMcpServer.Inputs;
using TodoMcpServer.InputValidator;
using TodoMcpServer.Interfaces;
using TodoMcpServer.Services;

namespace TodoMcpServer.Tools;

public class DeleteToDoListTool : ToolExecutor
{
    public override string Name { get; } = "DeleteToDoList";
    private readonly ToDoListArgumentValidator _argsValidator = new();
    private readonly ToDoLIstValidator _objectValidator = new();
    private readonly HttpClient _client;
    private readonly ITodoLookupService _lookupService;
    
    public DeleteToDoListTool(HttpClient client, ITodoLookupService lookupService)
    {
        _client = client;
        _lookupService = lookupService;
    }
    
    public override Tool GetToolDefinition()
    {
        return new Tool
        {
            Name = Name,
            Description = "Delets a new an existing ToDoList using its name",
            InputSchema = JsonSerializer.Deserialize<JsonElement>
            ("""
             {
             "type": "object",
                 "properties": {
                     "name": {
                         "type": "string",
                         "description": "The name of the ToDo list which will be deleted"
                     }
                 },
                 "required": ["name"]
             }
             """)
        };
    }
    
    protected override string GetSuccessMessage() => "Deletion of ToDo list was successful";
    protected override string GetFailureMessagePrefix() => "Deletion of ToDo list failed";
    
    protected override async ValueTask<HttpResponseMessage> Logic(CallToolRequest request, CancellationToken cancellationToken)
    {
        var toDoList = await ExtractToDoListAsync(request.Arguments, cancellationToken);
        return await DeleteToDoListAsync(toDoList, cancellationToken);
    }
    
    private async ValueTask<CreateToDoList> ExtractToDoListAsync(IReadOnlyDictionary<string, JsonElement> arguments, CancellationToken cancellationToken)
    {
        var extractor = new RequestExtractor<CreateToDoList>(
            _argsValidator,
            _objectValidator,
            args => new CreateToDoList
            {
                Name = args["name"].GetString()!,
            }
        );
        return await extractor.ExtractAsync(arguments, cancellationToken);
    }

    private async ValueTask<HttpResponseMessage> DeleteToDoListAsync(CreateToDoList toDoList, CancellationToken cancellationToken)
    {
        var listId = await _lookupService.GetListIdByNameAsync(toDoList.Name, cancellationToken);
        
        return await _client.DeleteAsync($"http://localhost:5083/api/todolists/{listId}", cancellationToken);
    }
}