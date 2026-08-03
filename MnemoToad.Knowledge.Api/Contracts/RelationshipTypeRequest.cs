using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Knowledge.Api.Contracts;

public record RelationshipTypeRequest([Required] string Name, string? InverseName, string? Description);
