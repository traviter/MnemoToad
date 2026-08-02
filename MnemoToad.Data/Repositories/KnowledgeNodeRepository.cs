using Microsoft.EntityFrameworkCore;
using MnemoToad.Data.Entities;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Data.Repositories;

public class KnowledgeNodeRepository : IKnowledgeNodeRepository
{
    private readonly AppDbContext _db;

    public KnowledgeNodeRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<KnowledgeNode>> GetAllAsync(Guid? nodeTypeId = null)
    {
        var query = _db.KnowledgeNode.AsQueryable();
        if (nodeTypeId is not null)
            query = query.Where(n => n.NodeTypeId == nodeTypeId);

        return query.OrderBy(n => n.CanonicalName).ToListAsync();
    }

    public async Task<KnowledgeNode?> GetByIdAsync(Guid id) =>
        await _db.KnowledgeNode.FindAsync(id);

    public Task<bool> ExistsByNodeTypeIdAsync(Guid nodeTypeId) =>
        _db.KnowledgeNode.AnyAsync(n => n.NodeTypeId == nodeTypeId);

    public Task AddAsync(KnowledgeNode knowledgeNode)
    {
        _db.KnowledgeNode.Add(knowledgeNode);
        return Task.CompletedTask;
    }

    public void Remove(KnowledgeNode knowledgeNode) => _db.KnowledgeNode.Remove(knowledgeNode);

    // NodeTypeId existence and NodeTypeId/CanonicalName uniqueness are both enforced by DB
    // constraints (the FK and the composite UNIQUE key) rather than pre-flighted before writing, so
    // a violation surfaces only here. Translate just those two known constraint failures into a
    // ValidationException (the service layer maps that to a 400); anything else (e.g. the DB being
    // unreachable) propagates unhandled and becomes a 500.
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
            throw new ValidationException("A KnowledgeNode with the same NodeType and CanonicalName already exists.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.ForeignKeyViolation
        })
        {
            throw new ValidationException("The specified NodeType does not exist.");
        }
    }
}
