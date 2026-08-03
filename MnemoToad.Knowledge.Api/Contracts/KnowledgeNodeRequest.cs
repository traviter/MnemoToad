using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Knowledge.Api.Contracts;

public record KnowledgeNodeRequest([RequiredGuid] Guid NodeTypeId, [Required] string CanonicalName, string? Description);
