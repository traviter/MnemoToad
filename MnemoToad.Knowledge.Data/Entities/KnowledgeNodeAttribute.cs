namespace MnemoToad.Knowledge.Data.Entities;

public class KnowledgeNodeAttribute
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid KnowledgeNodeId { get; set; }
    public Guid AttributeTypeId { get; set; }
    public string Value { get; set; } = string.Empty;
}
