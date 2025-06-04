namespace TodoMcpServer.Interfaces;

public interface ITodoLookupService
{
    Task<long> GetListIdByNameAsync(string name, CancellationToken cancellationToken);
    Task<long> GetItemIdByNameAsync(long listId, string name, CancellationToken cancellationToken);
}