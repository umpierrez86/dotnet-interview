using System.Text.Json;
using ModelContextProtocol.Protocol;
using TodoMcpServer.CallToolTypes;
using TodoMcpServer.DataExtractors;
using TodoMcpServer.Inputs;
using TodoMcpServer.InputValidator;
using TodoMcpServer.Interfaces;
using TodoMcpServer.Services;

namespace TodoMcpServer.Tools;

public class DeleteItemTool : ToolExecutor
{
    public override string Name { get; } = "DeleteItem";
    private readonly ItemIdentifierArgumentValidator _argsValidator = new();
    private readonly DeleteItemValidator _objectValidator = new();
    private readonly HttpClient _client;
    private readonly ITodoLookupService _lookupService;

    public DeleteItemTool(HttpClient client, ITodoLookupService lookupService)
    {
        _client = client;
        _lookupService = lookupService;
    }
    
    public override Tool GetToolDefinition()
    {
        return new Tool
        {
            Name = Name,
            Description = "Delete an already existing item.",
            InputSchema = JsonSerializer.Deserialize<JsonElement>
            ("""
             {
             "type": "object",
                 "properties": {
                     "name": {
                         "type": "string",
                         "description": "The name of the item to delete"
                     },
                     "listName": {
                          "type": "string",
                          "description": "The name of the list who owns the item"
                     }
                 },
                 "required": ["name", "listName"]
             }
             """)
        };
    }
    
    protected override string GetSuccessMessage() => "Deletion of item was successful";
    protected override string GetFailureMessagePrefix() => "Deletion of item failed";

    protected override async ValueTask<HttpResponseMessage> Logic(CallToolRequest request, CancellationToken cancellationToken)
    {
        var toDoList = await ExtractToDoListAsync(request.Arguments, cancellationToken);
        return await DeleteItemAsync(toDoList, cancellationToken);
    }
    
    private async ValueTask<DeleteItem> ExtractToDoListAsync(IReadOnlyDictionary<string, JsonElement> arguments, CancellationToken cancellationToken)
    {
        var extractor = new RequestExtractor<DeleteItem>(
            _argsValidator,
            _objectValidator,
            args => new DeleteItem
            {
                Name = args["name"].GetString()!,
                ListName = args["listName"].GetString()!,
            }
        );
        return await extractor.ExtractAsync(arguments, cancellationToken);
    }

    private async ValueTask<HttpResponseMessage> DeleteItemAsync(DeleteItem item, CancellationToken cancellationToken)
    {
        var listId = await _lookupService.GetListIdByNameAsync(item.ListName, cancellationToken);
        var itemId = await _lookupService.GetItemIdByNameAsync(listId, item.Name, cancellationToken);
        
        return await _client.DeleteAsync($"http://localhost:5083/api/todoLists/{listId}/items/{itemId}", 
            cancellationToken);
    }
}