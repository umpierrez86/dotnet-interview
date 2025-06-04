using System.Text.Json;

namespace TodoMcpServer.CallToolTypes;

public class CallToolRequest
{
    public string? Name { get; set; }
    public IReadOnlyDictionary<string, JsonElement>? Arguments { get; set; }
}