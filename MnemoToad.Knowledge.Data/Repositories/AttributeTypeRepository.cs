using Microsoft.EntityFrameworkCore;
using MnemoToad.Knowledge.Data.Entities;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Knowledge.Data.Repositories;

public class AttributeTypeRepository : IAttributeTypeRepository
{
    private readonly IAppDbContext _db;

    public AttributeTypeRepository(IAppDbContext db)
    {
        _db = db;
    }

    public Task<List<AttributeType>> GetAllAsync() =>
        _db.AttributeType.OrderBy(a => a.Name).ToListAsync();

    public async Task<AttributeType?> GetByIdAsync(Guid id) =>
        await _db.AttributeType.FindAsync(id);

    public async Task<AttributeType> CreateAsync(AttributeType attributeType)
    {
        _db.AttributeType.Add(attributeType);
        await SaveChangesAsync();
        return attributeType;
    }

    public async Task<AttributeType?> UpdateAsync(AttributeType attributeType)
    {
        var existing = await GetByIdAsync(attributeType.Id);
        if (existing is null) return null;

        existing.Name = attributeType.Name;
        existing.Description = attributeType.Description;
        await SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            return await _db.ExecuteDeleteAsync(_db.AttributeType.Where(a => a.Id == id)) > 0;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.ForeignKeyViolation
        })
        {
            throw new ValidationException("The AttributeType cannot be deleted because it is referenced by one or more KnowledgeNodeAttributes.");
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
            throw new ValidationException("An AttributeType with that name already exists.");
        }
    }
}
