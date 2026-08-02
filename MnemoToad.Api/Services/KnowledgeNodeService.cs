using MnemoToad.Data.Entities;
using MnemoToad.Data.Repositories;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Api.Services;

public class KnowledgeNodeService : IKnowledgeNodeService
{
    private readonly IKnowledgeNodeRepository _repository;

    public KnowledgeNodeService(IKnowledgeNodeRepository repository)
    {
        _repository = repository;
    }

    public Task<List<KnowledgeNode>> GetAllAsync(Guid? nodeTypeId = null) => _repository.GetAllAsync(nodeTypeId);

    public Task<KnowledgeNode?> GetByIdAsync(Guid id) => _repository.GetByIdAsync(id);

    public async Task<KnowledgeNode> CreateAsync(KnowledgeNode knowledgeNode)
    {
        Validate(knowledgeNode.NodeTypeId, knowledgeNode.CanonicalName);

        await _repository.AddAsync(knowledgeNode);
        await _repository.SaveChangesAsync();
        return knowledgeNode;
    }

    public async Task<KnowledgeNode?> UpdateAsync(KnowledgeNode knowledgeNode)
    {
        var existing = await _repository.GetByIdAsync(knowledgeNode.Id);
        if (existing is null) return null;

        Validate(knowledgeNode.NodeTypeId, knowledgeNode.CanonicalName);

        existing.NodeTypeId = knowledgeNode.NodeTypeId;
        existing.CanonicalName = knowledgeNode.CanonicalName;
        existing.Description = knowledgeNode.Description;
        await _repository.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var knowledgeNode = await _repository.GetByIdAsync(id);
        if (knowledgeNode is null) return false;

        _repository.Remove(knowledgeNode);
        await _repository.SaveChangesAsync();
        return true;
    }

    private static void Validate(Guid nodeTypeId, string canonicalName)
    {
        if (nodeTypeId == Guid.Empty)
            throw new ValidationException("NodeTypeId is required.");

        if (string.IsNullOrWhiteSpace(canonicalName))
            throw new ValidationException("CanonicalName is required.");
    }
}
