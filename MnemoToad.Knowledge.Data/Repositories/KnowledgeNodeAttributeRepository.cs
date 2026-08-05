using Microsoft.EntityFrameworkCore;
using MnemoToad.Knowledge.Data.Entities;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Knowledge.Data.Repositories;

public class KnowledgeNodeAttributeRepository : IKnowledgeNodeAttributeRepository
{
    private readonly IAppDbContext _db;

    public KnowledgeNodeAttributeRepository(IAppDbContext db)
    {
        _db = db;
    }

    public Task<List<KnowledgeNodeAttribute>> GetByNodeIdAsync(Guid knowledgeNodeId) =>
        _db.KnowledgeNodeAttribute.Where(a => a.KnowledgeNodeId == knowledgeNodeId).ToListAsync();

    public async Task<KnowledgeNodeAttribute> CreateAsync(KnowledgeNodeAttribute knowledgeNodeAttribute)
    {
        _db.KnowledgeNodeAttribute.Add(knowledgeNodeAttribute);
        await SaveChangesAsync();
        return knowledgeNodeAttribute;
    }

    public async Task<KnowledgeNodeAttribute?> UpdateAsync(KnowledgeNodeAttribute knowledgeNodeAttribute)
    {
        var existing = await _db.KnowledgeNodeAttribute.FindAsync(knowledgeNodeAttribute.Id);
        if (existing is null) return null;

        existing.KnowledgeNodeId = knowledgeNodeAttribute.KnowledgeNodeId;
        existing.AttributeTypeId = knowledgeNodeAttribute.AttributeTypeId;
        existing.Value = knowledgeNodeAttribute.Value;
        await SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id) =>
        await _db.ExecuteDeleteAsync(_db.KnowledgeNodeAttribute.Where(a => a.Id == id)) > 0;

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
            throw new ValidationException("An attribute of that type already exists for this KnowledgeNode.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.ForeignKeyViolation,
            ConstraintName: "fk_knowledge_node_attribute_knowledge_node_id"
        })
        {
            throw new ValidationException("The specified KnowledgeNode does not exist.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.ForeignKeyViolation,
            ConstraintName: "fk_knowledge_node_attribute_attribute_type_id"
        })
        {
            throw new ValidationException("The specified AttributeType does not exist.");
        }
    }
}
