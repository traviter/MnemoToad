namespace MnemoToad.Api.Contracts;

public record KnowledgeRelationRequest(Guid SourceNodeId, Guid RelationshipTypeId, Guid TargetNodeId);
