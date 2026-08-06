using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace MnemoToad.Knowledge.Api.Contracts;

public record KnowledgeNodeRequest(
    [RequiredGuid] Guid NodeTypeId,
    [Required] string CanonicalName,
    string? Description,
    Dictionary<string, JsonValue?>? Attributes = null);
