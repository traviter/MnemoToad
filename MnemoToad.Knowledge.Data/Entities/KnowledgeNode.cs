namespace MnemoToad.Knowledge.Data.Entities;

public class KnowledgeNode
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid NodeTypeId { get; set; }
    public string CanonicalName { get; set; } = string.Empty;
    public string? Description { get; set; }
}
