using Microsoft.EntityFrameworkCore;
using MnemoToad.Data.Entities;
using MnemoToad.Data.Repositories;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Api.Services;

public class RelationshipTypeService : IRelationshipTypeService
{
    private readonly IRelationshipTypeRepository _repository;

    public RelationshipTypeService(IRelationshipTypeRepository repository)
    {
        _repository = repository;
    }

    public Task<List<RelationshipType>> GetAllAsync() => _repository.GetAllAsync();

    public Task<RelationshipType?> GetByIdAsync(Guid id) => _repository.GetByIdAsync(id);

    public async Task<RelationshipType> CreateAsync(string name, string? inverseName, string? description)
    {
        Validate(name);

        var relationshipType = new RelationshipType
        {
            Id = Guid.NewGuid(),
            Name = name,
            InverseName = inverseName,
            Description = description
        };
        await _repository.AddAsync(relationshipType);
        await SaveChangesAsync();
        return relationshipType;
    }

    public async Task<RelationshipType?> UpdateAsync(Guid id, string name, string? inverseName, string? description)
    {
        var relationshipType = await _repository.GetByIdAsync(id);
        if (relationshipType is null) return null;

        Validate(name);

        relationshipType.Name = name;
        relationshipType.InverseName = inverseName;
        relationshipType.Description = description;
        await SaveChangesAsync();
        return relationshipType;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var relationshipType = await _repository.GetByIdAsync(id);
        if (relationshipType is null) return false;

        // TODO: guard against deleting a RelationshipType referenced by a Relationship, once
        // that table exists (no Relationship table/FK to check against yet).
        _repository.Remove(relationshipType);
        await SaveChangesAsync();
        return true;
    }

    private static void Validate(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Name is required.");
    }

    private async Task SaveChangesAsync()
    {
        try
        {
            await _repository.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation
        })
        {
            throw new ValidationException("A RelationshipType with that name already exists.");
        }
    }
}
