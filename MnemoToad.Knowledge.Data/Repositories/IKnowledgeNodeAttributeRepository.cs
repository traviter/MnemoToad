using MnemoToad.Knowledge.Data.Entities;

namespace MnemoToad.Knowledge.Data.Repositories;

public interface IKnowledgeNodeAttributeRepository
{
    Task<List<KnowledgeNodeAttribute>> GetByNodeIdAsync(Guid knowledgeNodeId);
    Task<KnowledgeNodeAttribute> CreateAsync(KnowledgeNodeAttribute knowledgeNodeAttribute);
    Task<KnowledgeNodeAttribute?> UpdateAsync(KnowledgeNodeAttribute knowledgeNodeAttribute);
    Task<bool> DeleteAsync(Guid id);
}
