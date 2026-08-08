using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Knowledge.Api.Contracts;

public record KnowledgeRelationRequest([Required] Guid? SourceNodeId, [Required] Guid? RelationshipTypeId, [Required] Guid? TargetNodeId);
