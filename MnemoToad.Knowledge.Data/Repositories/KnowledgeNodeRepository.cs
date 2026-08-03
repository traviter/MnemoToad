using Microsoft.EntityFrameworkCore;
using MnemoToad.Knowledge.Data.Entities;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Knowledge.Data.Repositories;

public class KnowledgeNodeRepository : IKnowledgeNodeRepository
{
    private readonly IAppDbContext _db;

    public KnowledgeNodeRepository(IAppDbContext db)
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

    public async Task<KnowledgeNode> CreateAsync(KnowledgeNode knowledgeNode)
    {
        _db.KnowledgeNode.Add(knowledgeNode);
        await SaveChangesAsync();
        return knowledgeNode;
    }

    public async Task<KnowledgeNode?> UpdateAsync(KnowledgeNode knowledgeNode)
    {
        var existing = await GetByIdAsync(knowledgeNode.Id);
        if (existing is null) return null;

        existing.NodeTypeId = knowledgeNode.NodeTypeId;
        existing.CanonicalName = knowledgeNode.CanonicalName;
        existing.Description = knowledgeNode.Description;
        await SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            return await _db.ExecuteDeleteAsync(_db.KnowledgeNode.Where(n => n.Id == id)) > 0;
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
    }
}
