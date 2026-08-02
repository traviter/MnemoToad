using MnemoToad.Data.Entities;
using MnemoToad.Data.Repositories;
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

    public async Task<RelationshipType> CreateAsync(RelationshipType relationshipType)
    {
        Validate(relationshipType.Name);

        await _repository.AddAsync(relationshipType);
        await _repository.SaveChangesAsync();
        return relationshipType;
    }

    public async Task<RelationshipType?> UpdateAsync(RelationshipType relationshipType)
    {
        var existing = await _repository.GetByIdAsync(relationshipType.Id);
        if (existing is null) return null;

        Validate(relationshipType.Name);

        existing.Name = relationshipType.Name;
        existing.InverseName = relationshipType.InverseName;
        existing.Description = relationshipType.Description;
        await _repository.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var relationshipType = await _repository.GetByIdAsync(id);
        if (relationshipType is null) return false;

        // TODO: guard against deleting a RelationshipType referenced by a Relationship, once
        // that table exists (no Relationship table/FK to check against yet).
        _repository.Remove(relationshipType);
        await _repository.SaveChangesAsync();
        return true;
    }

    private static void Validate(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Name is required.");
    }
}
