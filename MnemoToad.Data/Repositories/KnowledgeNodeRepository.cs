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

    public Task AddAsync(KnowledgeNode knowledgeNode)
    {
        _db.KnowledgeNode.Add(knowledgeNode);
        return Task.CompletedTask;
    }

    public void Remove(KnowledgeNode knowledgeNode) => _db.KnowledgeNode.Remove(knowledgeNode);

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
            SqlState: PostgresErrorCodes.ForeignKeyViolation,
            TableName: "knowledge_node"
        })
        {
            throw new ValidationException("The specified NodeType does not exist.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.ForeignKeyViolation,
            TableName: "knowledge_relation"
        })
        {
            throw new ValidationException("The KnowledgeNode cannot be deleted because it is referenced by one or more KnowledgeRelations.");
        }
    }
}
