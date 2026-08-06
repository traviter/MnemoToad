using Microsoft.EntityFrameworkCore;
using MnemoToad.Knowledge.Data.Entities;
using Npgsql;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

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

    public async Task<KnowledgeNode?> GetByIdAsync(Guid id)
    {
        var node = await _db.KnowledgeNode.FindAsync(id);
        if (node is null) return null;

        var rows = await _db.KnowledgeNodeAttribute.Where(a => a.KnowledgeNodeId == id).ToListAsync();
        node.Attributes = rows.ToDictionary(r => r.Key, r => r.Value);
        return node;
    }

    public async Task<KnowledgeNode> CreateAsync(KnowledgeNode knowledgeNode)
    {
        knowledgeNode.Attributes ??= new();
        _db.KnowledgeNode.Add(knowledgeNode);

        foreach (var (key, value) in knowledgeNode.Attributes)
        {
            _db.KnowledgeNodeAttribute.Add(new KnowledgeNodeAttribute
            {
                KnowledgeNodeId = knowledgeNode.Id,
                Key = key,
                Value = value
            });
        }

        await SaveChangesAsync();
        return knowledgeNode;
    }

    public async Task<KnowledgeNode?> UpdateAsync(KnowledgeNode knowledgeNode)
    {
        var existing = await _db.KnowledgeNode.FindAsync(knowledgeNode.Id);
        if (existing is null) return null;

        knowledgeNode.Attributes ??= new();
        existing.NodeTypeId = knowledgeNode.NodeTypeId;
        existing.CanonicalName = knowledgeNode.CanonicalName;
        existing.Description = knowledgeNode.Description;

        var currentRows = await _db.KnowledgeNodeAttribute.Where(a => a.KnowledgeNodeId == existing.Id).ToListAsync();

        foreach (var row in currentRows.Where(r => !knowledgeNode.Attributes.ContainsKey(r.Key)))
            _db.KnowledgeNodeAttribute.Remove(row);

        foreach (var (key, value) in knowledgeNode.Attributes)
        {
            var existingRow = currentRows.FirstOrDefault(r => r.Key == key);
            if (existingRow is not null)
                existingRow.Value = value;
            else
                _db.KnowledgeNodeAttribute.Add(new KnowledgeNodeAttribute { KnowledgeNodeId = existing.Id, Key = key, Value = value });
        }

        await SaveChangesAsync();
        existing.Attributes = knowledgeNode.Attributes;
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
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "uq_knowledge_node_node_type_id_canonical_name"
        })
        {
            throw new ValidationException("A KnowledgeNode with the same NodeType and CanonicalName already exists.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "pk_knowledge_node_attribute"
        })
        {
            throw new ValidationException("An attribute with that key already exists for this KnowledgeNode.");
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
