using MnemoToad.Data.Entities;
using MnemoToad.Data.Repositories;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Api.Services;

public class NodeTypeService : INodeTypeService
{
    private readonly INodeTypeRepository _repository;

    public NodeTypeService(INodeTypeRepository repository)
    {
        _repository = repository;
    }

    public Task<List<NodeType>> GetAllAsync() => _repository.GetAllAsync();

    public Task<NodeType?> GetByIdAsync(Guid id) => _repository.GetByIdAsync(id);

    public async Task<NodeType> CreateAsync(NodeType nodeType)
    {
        Validate(nodeType.Name);

        await _repository.AddAsync(nodeType);
        await _repository.SaveChangesAsync();
        return nodeType;
    }

    public async Task<NodeType?> UpdateAsync(NodeType nodeType)
    {
        var existing = await _repository.GetByIdAsync(nodeType.Id);
        if (existing is null) return null;

        Validate(nodeType.Name);

        existing.Name = nodeType.Name;
        existing.Description = nodeType.Description;
        await _repository.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var nodeType = await _repository.GetByIdAsync(id);
        if (nodeType is null) return false;

        _repository.Remove(nodeType);
        await _repository.SaveChangesAsync();
        return true;
    }

    private static void Validate(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Name is required.");
    }
}
