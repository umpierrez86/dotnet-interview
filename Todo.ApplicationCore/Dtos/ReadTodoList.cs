namespace TodoApi.Dtos;

public class ReadTodoList
{
    public long Id { get; set; }
    public string Name { get; set; }
    public List<ReadItem> Items { get; set; }
}