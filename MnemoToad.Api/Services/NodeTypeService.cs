using MnemoToad.Data.Entities;
using MnemoToad.Data.Repositories;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Api.Services;

public class NodeTypeService : INodeTypeService
{
    private readonly INodeTypeRepository _repository;
    private readonly IKnowledgeNodeRepository _knowledgeNodeRepository;

    public NodeTypeService(INodeTypeRepository repository, IKnowledgeNodeRepository knowledgeNodeRepository)
    {
        _repository = repository;
        _knowledgeNodeRepository = knowledgeNodeRepository;
    }

    public Task<List<NodeType>> GetAllAsync() => _repository.GetAllAsync();

    public Task<NodeType?> GetByIdAsync(Guid id) => _repository.GetByIdAsync(id);

    public async Task<NodeType> CreateAsync(NodeType nodeType)
    {
        if (string.IsNullOrWhiteSpace(nodeType.Name))
            throw new ValidationException("Name is required.");

        if (await _repository.ExistsWithNameAsync(nodeType.Name))
            throw new ValidationException($"A NodeType named '{nodeType.Name}' already exists.");

        await _repository.AddAsync(nodeType);
        await _repository.SaveChangesAsync();
        return nodeType;
    }

    public async Task<NodeType?> UpdateAsync(NodeType nodeType)
    {
        var existing = await _repository.GetByIdAsync(nodeType.Id);
        if (existing is null) return null;

        if (string.IsNullOrWhiteSpace(nodeType.Name))
            throw new ValidationException("Name is required.");

        if (await _repository.ExistsWithNameAsync(nodeType.Name, nodeType.Id))
            throw new ValidationException($"A NodeType named '{nodeType.Name}' already exists.");

        existing.Name = nodeType.Name;
        existing.Description = nodeType.Description;
        await _repository.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var nodeType = await _repository.GetByIdAsync(id);
        if (nodeType is null) return false;

        if (await _knowledgeNodeRepository.ExistsByNodeTypeIdAsync(id))
            throw new ValidationException($"NodeType '{nodeType.Name}' cannot be deleted because it is referenced by one or more KnowledgeNodes.");

        _repository.Remove(nodeType);
        await _repository.SaveChangesAsync();
        return true;
    }
}
