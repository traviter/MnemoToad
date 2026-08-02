using Microsoft.EntityFrameworkCore;
using MnemoToad.Data.Entities;
using MnemoToad.Data.Repositories;
using Npgsql;
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
        await SaveChangesAsync();
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
        await SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var knowledgeNode = await _repository.GetByIdAsync(id);
        if (knowledgeNode is null) return false;

        _repository.Remove(knowledgeNode);
        await SaveChangesAsync();
        return true;
    }

    private static void Validate(Guid nodeTypeId, string canonicalName)
    {
        if (nodeTypeId == Guid.Empty)
            throw new ValidationException("NodeTypeId is required.");

        if (string.IsNullOrWhiteSpace(canonicalName))
            throw new ValidationException("CanonicalName is required.");
    }

    // NodeTypeId existence and NodeTypeId/CanonicalName uniqueness are both enforced by DB
    // constraints (the FK and the composite UNIQUE key) rather than pre-flighted here, so a
    // violation surfaces only when SaveChangesAsync actually writes. Translate just those two
    // known constraint failures into a 400; anything else (e.g. the DB being unreachable)
    // propagates unhandled and becomes a 500.
    private async Task SaveChangesAsync()
    {
        try
        {
            await _repository.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation or PostgresErrorCodes.ForeignKeyViolation
        } postgresException)
        {
            throw new ValidationException(DescribeConstraintViolation(postgresException));
        }
    }

    private static string DescribeConstraintViolation(PostgresException ex) =>
        ex.SqlState switch
        {
            PostgresErrorCodes.UniqueViolation =>
                "A KnowledgeNode with the same NodeType and CanonicalName already exists.",
            PostgresErrorCodes.ForeignKeyViolation =>
                "The specified NodeType does not exist.",
            _ => "The request violates a database constraint."
        };
}
