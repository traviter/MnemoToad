using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Knowledge.Api.Contracts;

public record KnowledgeRelationRequest([RequiredGuid] Guid SourceNodeId, [RequiredGuid] Guid RelationshipTypeId, [RequiredGuid] Guid TargetNodeId);
