using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Api.Contracts;

public record RelationshipTypeRequest([Required] string Name, string? InverseName, string? Description);
