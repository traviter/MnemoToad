using Microsoft.EntityFrameworkCore;
using MnemoToad.Knowledge.Data.Entities;
using MnemoToad.Knowledge.Data.Repositories;
using MnemoToad.Knowledge.Tests.TestSupport;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Knowledge.Tests.Repositories;

[TestFixture]
public class KnowledgeRelationRepositoryTests
{
    private MockableAppDbContext _db = null!;
    private KnowledgeRelationRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        _db = new MockableAppDbContext();
        _repository = new KnowledgeRelationRepository(_db);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    [Test]
    public async Task GetByNodeIdAsync_ReturnsRelationsWhereNodeIsSourceOrTarget()
    {
        var nodeId = Guid.NewGuid();
        await _db.KnowledgeRelation.AddRangeAsync(
            new KnowledgeRelation { Id = Guid.NewGuid(), SourceNodeId = nodeId, RelationshipTypeId = Guid.NewGuid(), TargetNodeId = Guid.NewGuid() },
            new KnowledgeRelation { Id = Guid.NewGuid(), SourceNodeId = Guid.NewGuid(), RelationshipTypeId = Guid.NewGuid(), TargetNodeId = nodeId },
            new KnowledgeRelation { Id = Guid.NewGuid(), SourceNodeId = Guid.NewGuid(), RelationshipTypeId = Guid.NewGuid(), TargetNodeId = Guid.NewGuid() });
        await _db.SaveChangesAsync();

        var found = await _repository.GetByNodeIdAsync(nodeId);

        Assert.That(found, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task CreateAsync_PersistsAndReturnsKnowledgeRelation()
    {
        var knowledgeRelation = new KnowledgeRelation { Id = Guid.NewGuid(), SourceNodeId = Guid.NewGuid(), RelationshipTypeId = Guid.NewGuid(), TargetNodeId = Guid.NewGuid() };

        var created = await _repository.CreateAsync(knowledgeRelation);

        Assert.That(created, Is.SameAs(knowledgeRelation));
        Assert.That(await _db.KnowledgeRelation.FindAsync(knowledgeRelation.Id), Is.Not.Null);
    }

    [Test]
    public async Task DeleteAsync_WhenExists_RemovesKnowledgeRelationAndReturnsTrue()
    {
        var knowledgeRelation = new KnowledgeRelation { Id = Guid.NewGuid(), SourceNodeId = Guid.NewGuid(), RelationshipTypeId = Guid.NewGuid(), TargetNodeId = Guid.NewGuid() };
        await _db.KnowledgeRelation.AddAsync(knowledgeRelation);
        await _db.SaveChangesAsync();

        var result = await _repository.DeleteAsync(knowledgeRelation.Id);

        Assert.That(result, Is.True);
        Assert.That(await _db.KnowledgeRelation.AsNoTracking().FirstOrDefaultAsync(r => r.Id == knowledgeRelation.Id), Is.Null);
    }

    [Test]
    public async Task DeleteAsync_WhenNotFound_ReturnsFalse()
    {
        var result = await _repository.DeleteAsync(Guid.NewGuid());

        Assert.That(result, Is.False);
    }

    [Test]
    public void CreateAsync_OnUniqueViolation_ThrowsValidationExceptionWithDuplicateMessage()
    {
        _db.ThrowOnSaveChanges(PostgresExceptionFactory.UniqueViolation());

        var ex = Assert.ThrowsAsync<ValidationException>(() => _repository.CreateAsync(new KnowledgeRelation
        {
            Id = Guid.NewGuid(),
            SourceNodeId = Guid.NewGuid(),
            RelationshipTypeId = Guid.NewGuid(),
            TargetNodeId = Guid.NewGuid()
        }));

        Assert.That(ex!.Message, Is.EqualTo("A KnowledgeRelation with the same SourceNode, RelationshipType, and TargetNode already exists."));
    }

    [Test]
    public void CreateAsync_OnSourceNodeForeignKeyViolation_ThrowsValidationExceptionAboutSourceNode()
    {
        _db.ThrowOnSaveChanges(PostgresExceptionFactory.ForeignKeyViolation(constraintName: "fk_knowledge_relation_source_node_id"));

        var ex = Assert.ThrowsAsync<ValidationException>(() => _repository.CreateAsync(new KnowledgeRelation
        {
            Id = Guid.NewGuid(),
            SourceNodeId = Guid.NewGuid(),
            RelationshipTypeId = Guid.NewGuid(),
            TargetNodeId = Guid.NewGuid()
        }));

        Assert.That(ex!.Message, Is.EqualTo("The specified source KnowledgeNode does not exist."));
    }

    [Test]
    public void CreateAsync_OnTargetNodeForeignKeyViolation_ThrowsValidationExceptionAboutTargetNode()
    {
        _db.ThrowOnSaveChanges(PostgresExceptionFactory.ForeignKeyViolation(constraintName: "fk_knowledge_relation_target_node_id"));

        var ex = Assert.ThrowsAsync<ValidationException>(() => _repository.CreateAsync(new KnowledgeRelation
        {
            Id = Guid.NewGuid(),
            SourceNodeId = Guid.NewGuid(),
            RelationshipTypeId = Guid.NewGuid(),
            TargetNodeId = Guid.NewGuid()
        }));

        Assert.That(ex!.Message, Is.EqualTo("The specified target KnowledgeNode does not exist."));
    }

    [Test]
    public void CreateAsync_OnRelationshipTypeForeignKeyViolation_ThrowsValidationExceptionAboutRelationshipType()
    {
        _db.ThrowOnSaveChanges(PostgresExceptionFactory.ForeignKeyViolation(constraintName: "fk_knowledge_relation_relationship_type_id"));

        var ex = Assert.ThrowsAsync<ValidationException>(() => _repository.CreateAsync(new KnowledgeRelation
        {
            Id = Guid.NewGuid(),
            SourceNodeId = Guid.NewGuid(),
            RelationshipTypeId = Guid.NewGuid(),
            TargetNodeId = Guid.NewGuid()
        }));

        Assert.That(ex!.Message, Is.EqualTo("The specified RelationshipType does not exist."));
    }
}
