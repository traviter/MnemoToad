using MnemoToad.Data.Entities;

namespace MnemoToad.Data.Repositories;

public interface IKnowledgeNodeRepository
{
    Task<List<KnowledgeNode>> GetAllAsync(Guid? nodeTypeId = null);
    Task<KnowledgeNode?> GetByIdAsync(Guid id);
    Task AddAsync(KnowledgeNode knowledgeNode);
    void Remove(KnowledgeNode knowledgeNode);
    Task SaveChangesAsync();
}
