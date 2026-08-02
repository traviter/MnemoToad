using MnemoToad.Data.Entities;

namespace MnemoToad.Api.Services;

public interface IKnowledgeRelationService
{
    Task<List<KnowledgeRelation>> GetByNodeIdAsync(Guid nodeId);
    Task<KnowledgeRelation> CreateAsync(KnowledgeRelation knowledgeRelation);
    Task<bool> DeleteAsync(Guid id);
}
