namespace TodoApi.Models;

public class Item
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
}