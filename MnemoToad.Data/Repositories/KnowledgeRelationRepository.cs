using Microsoft.EntityFrameworkCore;
using MnemoToad.Data.Entities;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Data.Repositories;

public class KnowledgeRelationRepository : IKnowledgeRelationRepository
{
    private readonly IAppDbContext _db;

    public KnowledgeRelationRepository(IAppDbContext db)
    {
        _db = db;
    }

    public Task<List<KnowledgeRelation>> GetByNodeIdAsync(Guid nodeId) =>
        _db.KnowledgeRelation
            .Where(r => r.SourceNodeId == nodeId || r.TargetNodeId == nodeId)
            .ToListAsync();

    public async Task<KnowledgeRelation> CreateAsync(KnowledgeRelation knowledgeRelation)
    {
        _db.KnowledgeRelation.Add(knowledgeRelation);
        await SaveChangesAsync();
        return knowledgeRelation;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var knowledgeRelation = await _db.KnowledgeRelation.FindAsync(id);
        if (knowledgeRelation is null) return false;

        _db.KnowledgeRelation.Remove(knowledgeRelation);
        await SaveChangesAsync();
        return true;
    }

    private async Task SaveChangesAsync()
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
