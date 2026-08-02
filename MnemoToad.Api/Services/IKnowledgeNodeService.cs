using MnemoToad.Data.Entities;

namespace MnemoToad.Api.Services;

public interface IKnowledgeNodeService
{
    Task<List<KnowledgeNode>> GetAllAsync(Guid? nodeTypeId = null);
    Task<KnowledgeNode?> GetByIdAsync(Guid id);
    Task<KnowledgeNode> CreateAsync(KnowledgeNode knowledgeNode);
    Task<KnowledgeNode?> UpdateAsync(KnowledgeNode knowledgeNode);
    Task<bool> DeleteAsync(Guid id);
}
