using MnemoToad.Data.Entities;

namespace MnemoToad.Api.Services;

public interface IRelationshipTypeService
{
    Task<List<RelationshipType>> GetAllAsync();
    Task<RelationshipType?> GetByIdAsync(Guid id);
    Task<RelationshipType> CreateAsync(RelationshipType relationshipType);
    Task<RelationshipType?> UpdateAsync(RelationshipType relationshipType);
    Task<bool> DeleteAsync(Guid id);
}
