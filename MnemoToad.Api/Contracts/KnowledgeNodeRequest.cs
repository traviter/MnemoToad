namespace MnemoToad.Api.Contracts
{
    public record KnowledgeNodeRequest(Guid NodeTypeId, string CanonicalName, string? Description);
}
