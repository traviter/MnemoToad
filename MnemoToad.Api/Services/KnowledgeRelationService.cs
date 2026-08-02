using MnemoToad.Data.Entities;
using MnemoToad.Data.Repositories;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Api.Services;

public class KnowledgeRelationService : IKnowledgeRelationService
{
    private readonly IKnowledgeRelationRepository _repository;

    public KnowledgeRelationService(IKnowledgeRelationRepository repository)
    {
        _repository = repository;
    }

    public Task<List<KnowledgeRelation>> GetByNodeIdAsync(Guid nodeId) => _repository.GetByNodeIdAsync(nodeId);

    public async Task<KnowledgeRelation> CreateAsync(KnowledgeRelation knowledgeRelation)
    {
        Validate(knowledgeRelation.SourceNodeId, knowledgeRelation.RelationshipTypeId, knowledgeRelation.TargetNodeId);

        await _repository.AddAsync(knowledgeRelation);
        await _repository.SaveChangesAsync();
        return knowledgeRelation;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var knowledgeRelation = await _repository.GetByIdAsync(id);
        if (knowledgeRelation is null) return false;

        _repository.Remove(knowledgeRelation);
        await _repository.SaveChangesAsync();
        return true;
    }

    private static void Validate(Guid sourceNodeId, Guid relationshipTypeId, Guid targetNodeId)
    {
        if (sourceNodeId == Guid.Empty)
            throw new ValidationException("SourceNodeId is required.");

        if (relationshipTypeId == Guid.Empty)
            throw new ValidationException("RelationshipTypeId is required.");

        if (targetNodeId == Guid.Empty)
            throw new ValidationException("TargetNodeId is required.");
    }
}
