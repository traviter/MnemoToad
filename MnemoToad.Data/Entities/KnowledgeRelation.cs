namespace MnemoToad.Data.Entities;

public class KnowledgeRelation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SourceNodeId { get; set; }
    public Guid RelationshipTypeId { get; set; }
    public Guid TargetNodeId { get; set; }
}
