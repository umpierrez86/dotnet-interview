using System.Text;
using System.Text.Json;
using ModelContextProtocol.Protocol;
using TodoMcpServer.CallToolTypes;
using TodoMcpServer.DataExtractors;
using TodoMcpServer.Inputs;
using TodoMcpServer.InputValidator;
using TodoMcpServer.Services;

namespace TodoMcpServer.Tools;

public class CreateToDoListTool : ToolExecutor
{
    public override string Name { get; } = "CreateToDoList";
    private readonly ToDoListArgumentValidator _argsValidator = new();
    private readonly ToDoLIstValidator _objectValidator = new();
    private readonly HttpClient _client;

    public CreateToDoListTool(HttpClient client)
    {
        _client = client;
    }

    public override Tool GetToolDefinition()
    {
        return new Tool
        {
            Name = Name,
            Description = "Creates a new ToDo list. Use this only when the list does not already exist.",
            InputSchema = JsonSerializer.Deserialize<JsonElement>
            ("""
             {
             "type": "object",
                 "properties": {
                     "name": {
                         "type": "string",
                         "description": "The name of the ToDo list"
                     }
                 },
                 "required": ["name"]
             }
             """)
        };
    }
    
    protected override string GetSuccessMessage() => "Creation of ToDo list was successful";
    protected override string GetFailureMessagePrefix() => "Creation of ToDo list failed";
    
    protected override async ValueTask<HttpResponseMessage> Logic(CallToolRequest request, CancellationToken cancellationToken)
    {
        var toDoList = await ExtractToDoListAsync(request.Arguments, cancellationToken);
        return await CreateToDoListAsync(toDoList, cancellationToken);
    }
    
    private async ValueTask<CreateToDoList> ExtractToDoListAsync(IReadOnlyDictionary<string, JsonElement> arguments, CancellationToken cancellationToken)
    {
        var extractor = new RequestExtractor<CreateToDoList>(
            _argsValidator,
            _objectValidator,
            args => new CreateToDoList { Name = args["name"].GetString()! }
        );
        return await extractor.ExtractAsync(arguments, cancellationToken);
    }

    private async ValueTask<HttpResponseMessage> CreateToDoListAsync(CreateToDoList toDoList, CancellationToken cancellationToken)
    {
        var payload = new { Name = toDoList.Name };
        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );
        return await _client.PostAsync("http://localhost:5083/api/todolists", content, cancellationToken);
    }

}