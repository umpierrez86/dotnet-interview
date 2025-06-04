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

public class CompleteItemTool : ToolExecutor
{
    public override string Name { get; } = "CompleteItem";
    private readonly ItemIdentifierArgumentValidator _argsValidator = new();
    private readonly UpdateItemValidator _objectValidator = new();
    private readonly HttpClient _client;
    private readonly ITodoLookupService _lookupService;

    public CompleteItemTool(HttpClient client, ITodoLookupService lookupService)
    {
        _client = client;
        _lookupService = lookupService;
    }
    
    public override Tool GetToolDefinition()
    {
        return new Tool
        {
            Name = Name,
            Description = "Marks an item as complete",
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
                     }
                 },
                 "required": ["name", "listName"]
             }
             """)
        };
    }

    protected override async ValueTask<HttpResponseMessage> Logic(CallToolRequest request, CancellationToken cancellationToken)
    {
        var item = await ExtractData(request.Arguments, cancellationToken);
        return await UpdateItemAsync(item, cancellationToken);
    }

    protected override string GetSuccessMessage() => "Item has been marked as complete.";
    protected override string GetFailureMessagePrefix() => "Failed to mark an item as complete.";

    private async ValueTask<UpdateItem> ExtractData(IReadOnlyDictionary<string, JsonElement> arguments, CancellationToken cancellationToken)
    {
        var argsValidation = await _argsValidator.ValidateAsync(arguments, cancellationToken);
        if (!argsValidation.IsValid)
        {
            throw new ArgumentException(string.Join(", ", argsValidation.Errors.Select(e => e.ErrorMessage)));
        }

        return new UpdateItem
        {
            Name = arguments["name"].GetString()!,
            ListName = arguments["listName"].GetString()!
        };
    }

    private async ValueTask<HttpResponseMessage> UpdateItemAsync(UpdateItem item, CancellationToken cancellationToken)
    {
        var listId = await _lookupService.GetListIdByNameAsync(item.ListName, cancellationToken);
        var itemId = await _lookupService.GetItemIdByNameAsync(listId, item.Name, cancellationToken);
        
        var emptyContent = new StringContent(string.Empty);

        return await _client.PatchAsync($"http://localhost:5083/api/todoLists/{listId}/items/{itemId}/complete", 
            emptyContent, cancellationToken);
    }
}