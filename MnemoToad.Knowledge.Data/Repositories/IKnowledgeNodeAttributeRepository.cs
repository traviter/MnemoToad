using System.Text.Json.Nodes;

namespace MnemoToad.Knowledge.Data.Repositories;

public interface IKnowledgeNodeAttributeRepository
{
    Task<Dictionary<string, JsonValue?>> GetByNodeIdAsync(Guid knowledgeNodeId);
}
