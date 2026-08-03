using MnemoToad.Data.Entities;

namespace MnemoToad.Data.Repositories;

public interface INodeTypeRepository
{
    Task<List<NodeType>> GetAllAsync();
    Task<NodeType?> GetByIdAsync(Guid id);
    Task<NodeType> CreateAsync(NodeType nodeType);
    Task<NodeType?> UpdateAsync(NodeType nodeType);
    Task<bool> DeleteAsync(Guid id);
}
