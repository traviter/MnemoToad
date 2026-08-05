using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Knowledge.Api.Contracts;

public record KnowledgeNodeAttributeRequest([RequiredGuid] Guid KnowledgeNodeId, [RequiredGuid] Guid AttributeTypeId, [Required] string Value);
