namespace TodoApi.Dtos;

public class ReadItem
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool isComplete { get; set; }
}