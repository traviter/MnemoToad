using Microsoft.EntityFrameworkCore;
using MnemoToad.Data.Entities;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Data.Repositories;

public class KnowledgeRelationRepository : IKnowledgeRelationRepository
{
    private readonly AppDbContext _db;

    public KnowledgeRelationRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<KnowledgeRelation>> GetByNodeIdAsync(Guid nodeId) =>
        _db.KnowledgeRelation
            .Where(r => r.SourceNodeId == nodeId || r.TargetNodeId == nodeId)
            .ToListAsync();

    public async Task<KnowledgeRelation?> GetByIdAsync(Guid id) =>
        await _db.KnowledgeRelation.FindAsync(id);

    public Task AddAsync(KnowledgeRelation knowledgeRelation)
    {
        _db.KnowledgeRelation.Add(knowledgeRelation);
        return Task.CompletedTask;
    }

    public void Remove(KnowledgeRelation knowledgeRelation) => _db.KnowledgeRelation.Remove(knowledgeRelation);

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
            throw new ValidationException("A KnowledgeRelation with the same SourceNode, RelationshipType, and TargetNode already exists.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.ForeignKeyViolation,
            ConstraintName: "fk_knowledge_relation_source_node_id"
        })
        {
            throw new ValidationException("The specified source KnowledgeNode does not exist.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.ForeignKeyViolation,
            ConstraintName: "fk_knowledge_relation_target_node_id"
        })
        {
            throw new ValidationException("The specified target KnowledgeNode does not exist.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.ForeignKeyViolation,
            ConstraintName: "fk_knowledge_relation_relationship_type_id"
        })
        {
            throw new ValidationException("The specified RelationshipType does not exist.");
        }
    }
}
