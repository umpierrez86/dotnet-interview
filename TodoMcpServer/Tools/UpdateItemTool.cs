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

public class UpdateItemTool : ToolExecutor
{
    public override string Name { get; } = "UpdateItem";
    private readonly ItemIdentifierArgumentValidator _argsValidator = new();
    private readonly UpdateItemValidator _objectValidator = new();
    private readonly HttpClient _client;
    private readonly ITodoLookupService _lookupService;

    public UpdateItemTool(HttpClient client, ITodoLookupService lookupService)
    {
        _client = client;
        _lookupService = lookupService;
    }
    public override Tool GetToolDefinition()
    {
        return new Tool
        {
            Name = Name,
            Description = "Updates an already existing item.",
            InputSchema = JsonSerializer.Deserialize<JsonElement>
            ("""
             {
             "type": "object",
                 "properties": {
                     "name": {
                         "type": "string",
                         "description": "The name of the item to update"
                     },
                     "listName": {
                          "type": "string",
                          "description": "The name of the list who owns the item"
                     },
                     "newName": {
                          "type": "string",
                          "description": "New name for the item. Empty is the user doesn't specify a new name."
                     },
                     "description": {
                          "type": "string",
                          "description": "The new description for the item. Empty is the user doesn't specify a new description."
                     }
                 },
                 "required": ["name", "listName", "newName", "description"]
             }
             """)
        };
    }
    
    protected override string GetSuccessMessage() => "Update of item was successful";
    protected override string GetFailureMessagePrefix() => "Update of item failed";

    protected override async ValueTask<HttpResponseMessage> Logic(CallToolRequest request, CancellationToken cancellationToken)
    {
        var toDoList = await ExtractData(request.Arguments, cancellationToken);
        return await UpdateItemAsync(toDoList, cancellationToken);
    }
    
    private async ValueTask<UpdateItem> ExtractData(IReadOnlyDictionary<string, JsonElement> arguments, CancellationToken cancellationToken)
    {
        var extractor = new RequestExtractor<UpdateItem>(
            _argsValidator,
            _objectValidator,
            args => new UpdateItem
            {
                Name = args["name"].GetString()!,
                NewName = args["newName"].GetString()!,
                Description = args["description"].GetString()!,
                ListName = args["listName"].GetString()!,
            }
        );
        return await extractor.ExtractAsync(arguments, cancellationToken);
    }

    private async ValueTask<HttpResponseMessage> UpdateItemAsync(UpdateItem item, CancellationToken cancellationToken)
    {
        var listId = await _lookupService.GetListIdByNameAsync(item.ListName, cancellationToken);
        var itemId = await _lookupService.GetItemIdByNameAsync(listId, item.Name, cancellationToken);
        
        var payload = new
        {
            Name = item.NewName,
            Description = item.Description,
            IsComplete = item.Completed
        };

        var putContent = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        return await _client.PatchAsync($"http://localhost:5083/api/todoLists/{listId}/items/{itemId}", 
            putContent, cancellationToken);
    }
}