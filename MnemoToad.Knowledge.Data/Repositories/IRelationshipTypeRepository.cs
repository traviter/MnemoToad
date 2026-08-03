using MnemoToad.Knowledge.Data.Entities;

namespace MnemoToad.Knowledge.Data.Repositories;

public interface IRelationshipTypeRepository
{
    Task<List<RelationshipType>> GetAllAsync();
    Task<RelationshipType?> GetByIdAsync(Guid id);
    Task<RelationshipType> CreateAsync(RelationshipType relationshipType);
    Task<RelationshipType?> UpdateAsync(RelationshipType relationshipType);
    Task<bool> DeleteAsync(Guid id);
}
