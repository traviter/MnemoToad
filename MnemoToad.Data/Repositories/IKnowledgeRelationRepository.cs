using MnemoToad.Data.Entities;

namespace MnemoToad.Data.Repositories;

public interface IKnowledgeRelationRepository
{
    Task<List<KnowledgeRelation>> GetByNodeIdAsync(Guid nodeId);
    Task<KnowledgeRelation?> GetByIdAsync(Guid id);
    Task AddAsync(KnowledgeRelation knowledgeRelation);
    void Remove(KnowledgeRelation knowledgeRelation);
    Task SaveChangesAsync();
}
