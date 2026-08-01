using MnemoToad.Data.Entities;

namespace MnemoToad.Api.Services
{
    public interface IKnowledgeNodeService
    {
        Task<List<KnowledgeNode>> GetAllAsync(Guid? nodeTypeId = null);
        Task<KnowledgeNode?> GetByIdAsync(Guid id);
        Task<KnowledgeNode> CreateAsync(Guid nodeTypeId, string canonicalName, string? description);
        Task<KnowledgeNode?> UpdateAsync(Guid id, Guid nodeTypeId, string canonicalName, string? description);
        Task<bool> DeleteAsync(Guid id);
    }
}
