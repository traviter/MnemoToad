namespace MnemoToad.Data.Entities;

public class RelationshipType
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? InverseName { get; set; }
    public string? Description { get; set; }
}
