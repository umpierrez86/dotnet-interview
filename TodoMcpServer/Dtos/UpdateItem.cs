namespace TodoMcpServer.Inputs;

public class UpdateItem
{
    public string Name { get; set; }
    public string NewName { get; set; }
    public string Description { get; set; }
    public bool Completed { get; set; }
    public string ListName { get; set; }
}