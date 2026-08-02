namespace MnemoToad.Api.Contracts;

public record RelationshipTypeRequest(string Name, string? InverseName, string? Description);
