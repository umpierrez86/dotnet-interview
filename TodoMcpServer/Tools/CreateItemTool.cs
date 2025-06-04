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

public class CreateItemTool : ToolExecutor
{
    public override string Name { get; } = "CreateItem";
    private readonly HttpClient _client;
    private readonly ItemArgumentValidator _argsValidator = new();
    private readonly ItemValidator _objectValidator = new();
    private readonly ITodoLookupService _lookupService;

    public CreateItemTool(HttpClient client, ITodoLookupService lookupService)
    {
        _client = client;
        _lookupService = lookupService;
    }

    public override Tool GetToolDefinition()
    {
        return new Tool
        {
            Name = Name,
            Description = "Creates a new item that belongs to a list. Use this only when the item does not already exist.",
            InputSchema = JsonSerializer.Deserialize<JsonElement>
            ("""
             {
             "type": "object",
                 "properties": {
                     "name": {
                         "type": "string",
                         "description": "The name of the item"
                     },
                     "listName": {
                        "type": "string",
                        "description": "The name of the list to which the item belongs to."
                     },
                     "description": {
                        "type": "string",
                        "description": "The description of the item"
                     }
                 },
                 "required": ["name","listName","description"]
             }
             """)
        };
    }
    
    protected override string GetSuccessMessage() => "The item was created successfully.";
    protected override string GetFailureMessagePrefix() => "There was an error creating the item.";
    
    protected override async ValueTask<HttpResponseMessage> Logic(CallToolRequest request, CancellationToken cancellationToken)
    {
        var toDoList = await ExtractData(request.Arguments, cancellationToken);
        return await CreateItemAsync(toDoList, cancellationToken);
    }
    
    private async ValueTask<CreateItem> ExtractData(IReadOnlyDictionary<string, JsonElement> arguments, CancellationToken cancellationToken)
    {
        var extractor = new RequestExtractor<CreateItem>(
            _argsValidator,
            _objectValidator,
            args => new CreateItem
            {
                Name = args["name"].GetString()!,
                Description = args["description"].GetString()!,
                ListName = args["listName"].GetString()!,
            }
        );
        return await extractor.ExtractAsync(arguments, cancellationToken);
    }

    private async ValueTask<HttpResponseMessage> CreateItemAsync(CreateItem item, CancellationToken cancellationToken)
    {
        var listId = await _lookupService.GetListIdByNameAsync(item.ListName, cancellationToken);
        
        var payload = new
        {
            Name = item.Name,
            Description = item.Description,
        };

        var createContent = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        return await _client.PostAsync($"http://localhost:5083/api/todoLists/{listId}/items", 
            createContent, cancellationToken);
    }
}