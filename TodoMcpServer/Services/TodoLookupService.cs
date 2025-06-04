using System.Text.Json;
using TodoMcpServer.Dtos;
using TodoMcpServer.Interfaces;

namespace TodoMcpServer.Services;

public class TodoLookupService : ITodoLookupService
{
    private readonly HttpClient _client;

    public TodoLookupService(HttpClient client)
    {
        _client = client;
    }

    public async Task<long> GetListIdByNameAsync(string name, CancellationToken cancellationToken)
    {
        var response = await _client.GetAsync($"http://localhost:5083/api/todoLists?name={name}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new ArgumentException("Failed to retrieve list");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var lists = JsonSerializer.Deserialize<List<EntityId>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return lists?.FirstOrDefault()?.Id
               ?? throw new ArgumentException("No list was found with the name: " + name);
    }

    public async Task<long> GetItemIdByNameAsync(long listId, string name, CancellationToken cancellationToken)
    {
        var response = await _client.GetAsync($"http://localhost:5083/api/todoLists/{listId}/items?name={name}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new ArgumentException("Failed to retrieve item");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var items = JsonSerializer.Deserialize<List<EntityId>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return items?.FirstOrDefault()?.Id
               ?? throw new ArgumentException("No item was found with the name: " + name);
    }
}