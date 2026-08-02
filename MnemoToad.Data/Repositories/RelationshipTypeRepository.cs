using Microsoft.EntityFrameworkCore;
using MnemoToad.Data.Entities;

namespace MnemoToad.Data.Repositories;

public class RelationshipTypeRepository : IRelationshipTypeRepository
{
    private readonly AppDbContext _db;

    public RelationshipTypeRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<RelationshipType>> GetAllAsync() =>
        _db.RelationshipType.OrderBy(r => r.Name).ToListAsync();

    public async Task<RelationshipType?> GetByIdAsync(Guid id) =>
        await _db.RelationshipType.FindAsync(id);

    public Task AddAsync(RelationshipType relationshipType)
    {
        _db.RelationshipType.Add(relationshipType);
        return Task.CompletedTask;
    }

    public void Remove(RelationshipType relationshipType) => _db.RelationshipType.Remove(relationshipType);

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}
