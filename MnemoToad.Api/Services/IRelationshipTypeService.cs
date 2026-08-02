using MnemoToad.Data.Entities;

namespace MnemoToad.Api.Services;

public interface IRelationshipTypeService
{
    Task<List<RelationshipType>> GetAllAsync();
    Task<RelationshipType?> GetByIdAsync(Guid id);
    Task<RelationshipType> CreateAsync(string name, string? inverseName, string? description);
    Task<RelationshipType?> UpdateAsync(Guid id, string name, string? inverseName, string? description);
    Task<bool> DeleteAsync(Guid id);
}
