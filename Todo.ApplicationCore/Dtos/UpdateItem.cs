namespace TodoApi.Dtos;

public class UpdateItem
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsComplete { get; set; }
}