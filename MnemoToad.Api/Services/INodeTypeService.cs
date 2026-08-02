using MnemoToad.Data.Entities;

namespace MnemoToad.Api.Services;

public interface INodeTypeService
{
    Task<List<NodeType>> GetAllAsync();
    Task<NodeType?> GetByIdAsync(Guid id);
    Task<NodeType> CreateAsync(NodeType nodeType);
    Task<NodeType?> UpdateAsync(NodeType nodeType);
    Task<bool> DeleteAsync(Guid id);
}
