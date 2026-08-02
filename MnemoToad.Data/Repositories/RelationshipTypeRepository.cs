using Microsoft.EntityFrameworkCore;
using MnemoToad.Data.Entities;
using Npgsql;
using System.ComponentModel.DataAnnotations;

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

    public async Task SaveChangesAsync()
    {
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation
        })
        {
            throw new ValidationException("A RelationshipType with that name already exists.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.ForeignKeyViolation,
            TableName: "knowledge_relation"
        })
        {
            throw new ValidationException("The RelationshipType cannot be deleted because it is referenced by one or more KnowledgeRelations.");
        }
    }
}
