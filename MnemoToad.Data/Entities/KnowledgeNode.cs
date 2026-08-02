namespace MnemoToad.Data.Entities;

public class KnowledgeNode
{
    public Guid Id { get; set; }
    public Guid NodeTypeId { get; set; }
    public string CanonicalName { get; set; } = string.Empty;
    public string? Description { get; set; }
}
