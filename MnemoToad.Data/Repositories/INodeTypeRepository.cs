using MnemoToad.Data.Entities;

namespace MnemoToad.Data.Repositories;

public interface INodeTypeRepository
{
    Task<List<NodeType>> GetAllAsync();
    Task<NodeType?> GetByIdAsync(Guid id);
    Task<bool> ExistsWithNameAsync(string name, Guid? excludingId = null);
    Task AddAsync(NodeType nodeType);
    void Remove(NodeType nodeType);
    Task SaveChangesAsync();
}
