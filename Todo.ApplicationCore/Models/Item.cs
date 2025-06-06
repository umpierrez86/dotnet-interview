namespace TodoApi.Models;

public class Item
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public bool IsComplete { get; set; } = false;
    public long TodoListId { get; set; }
}