namespace TodoMcpServer.Inputs;

public class CreateItem
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string ListName { get; set; }
}