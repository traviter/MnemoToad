using MnemoToad.Data.Entities;

namespace MnemoToad.Data.Repositories;

public interface IKnowledgeRelationRepository
{
    Task<List<KnowledgeRelation>> GetByNodeIdAsync(Guid nodeId);
    Task<KnowledgeRelation> CreateAsync(KnowledgeRelation knowledgeRelation);
    Task<bool> DeleteAsync(Guid id);
}
