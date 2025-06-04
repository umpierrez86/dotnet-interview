namespace TodoApi.Dtos;

public class CreateItem
{
    public required string Name { get; set; }
    public required string Description { get; set; }
}