namespace MnemoToad.Data.Entities;

public class NodeType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
