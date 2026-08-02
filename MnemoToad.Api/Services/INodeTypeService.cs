using MnemoToad.Data.Entities;

namespace MnemoToad.Api.Services;

public interface INodeTypeService
{
    Task<List<NodeType>> GetAllAsync();
    Task<NodeType?> GetByIdAsync(Guid id);
    Task<NodeType> CreateAsync(string name, string? description);
    Task<NodeType?> UpdateAsync(Guid id, string name, string? description);
    Task<bool> DeleteAsync(Guid id);
}
