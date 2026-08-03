using MnemoToad.Data.Entities;

namespace MnemoToad.Data.Repositories;

public interface IRelationshipTypeRepository
{
    Task<List<RelationshipType>> GetAllAsync();
    Task<RelationshipType?> GetByIdAsync(Guid id);
    Task<RelationshipType> CreateAsync(RelationshipType relationshipType);
    Task<RelationshipType?> UpdateAsync(RelationshipType relationshipType);
    Task<bool> DeleteAsync(Guid id);
}
