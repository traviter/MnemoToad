using MnemoToad.Data.Entities;

namespace MnemoToad.Data.Repositories;

public interface IRelationshipTypeRepository
{
    Task<List<RelationshipType>> GetAllAsync();
    Task<RelationshipType?> GetByIdAsync(Guid id);
    Task AddAsync(RelationshipType relationshipType);
    void Remove(RelationshipType relationshipType);
    Task SaveChangesAsync();
}
