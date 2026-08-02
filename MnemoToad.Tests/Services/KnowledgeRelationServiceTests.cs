using Moq;
using MnemoToad.Api.Services;
using MnemoToad.Data.Entities;
using MnemoToad.Data.Repositories;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Tests.Services;

[TestFixture]
public class KnowledgeRelationServiceTests
{
    private Mock<IKnowledgeRelationRepository> _repository = null!;
    private KnowledgeRelationService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new Mock<IKnowledgeRelationRepository>();
        _service = new KnowledgeRelationService(_repository.Object);
    }

    [Test]
    public async Task CreateAsync_WithValidData_ReturnsCreatedKnowledgeRelation()
    {
        var sourceNodeId = Guid.NewGuid();
        var relationshipTypeId = Guid.NewGuid();
        var targetNodeId = Guid.NewGuid();

        var created = await _service.CreateAsync(new KnowledgeRelation
        {
            SourceNodeId = sourceNodeId,
            RelationshipTypeId = relationshipTypeId,
            TargetNodeId = targetNodeId
        });

        Assert.That(created.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(created.SourceNodeId, Is.EqualTo(sourceNodeId));
        Assert.That(created.RelationshipTypeId, Is.EqualTo(relationshipTypeId));
        Assert.That(created.TargetNodeId, Is.EqualTo(targetNodeId));
        _repository.Verify(r => r.AddAsync(It.Is<KnowledgeRelation>(k => k.SourceNodeId == sourceNodeId)), Times.Once);
        _repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void CreateAsync_WithEmptySourceNodeId_ThrowsValidationException()
    {
        Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(new KnowledgeRelation
        {
            SourceNodeId = Guid.Empty,
            RelationshipTypeId = Guid.NewGuid(),
            TargetNodeId = Guid.NewGuid()
        }));
        _repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Test]
    public void CreateAsync_WithEmptyRelationshipTypeId_ThrowsValidationException()
    {
        Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(new KnowledgeRelation
        {
            SourceNodeId = Guid.NewGuid(),
            RelationshipTypeId = Guid.Empty,
            TargetNodeId = Guid.NewGuid()
        }));
        _repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Test]
    public void CreateAsync_WithEmptyTargetNodeId_ThrowsValidationException()
    {
        Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(new KnowledgeRelation
        {
            SourceNodeId = Guid.NewGuid(),
            RelationshipTypeId = Guid.NewGuid(),
            TargetNodeId = Guid.Empty
        }));
        _repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    // Constraint-violation translation (duplicate relation, or a source/target/relationship type
    // that doesn't exist) happens in KnowledgeRelationRepository.SaveChangesAsync(), not here — the
    // service has nothing left to translate, it just needs to not swallow whatever the repository
    // throws.
    [Test]
    public void CreateAsync_WhenRepositoryThrowsValidationException_PropagatesException()
    {
        _repository.Setup(r => r.SaveChangesAsync())
            .ThrowsAsync(new ValidationException("A KnowledgeRelation with the same SourceNode, RelationshipType, and TargetNode already exists."));

        Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(new KnowledgeRelation
        {
            SourceNodeId = Guid.NewGuid(),
            RelationshipTypeId = Guid.NewGuid(),
            TargetNodeId = Guid.NewGuid()
        }));
    }

    [Test]
    public void CreateAsync_WhenRepositoryThrowsUnrelatedException_PropagatesException()
    {
        _repository.Setup(r => r.SaveChangesAsync())
            .ThrowsAsync(new InvalidOperationException("could not connect to server"));

        Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(new KnowledgeRelation
        {
            SourceNodeId = Guid.NewGuid(),
            RelationshipTypeId = Guid.NewGuid(),
            TargetNodeId = Guid.NewGuid()
        }));
    }

    [Test]
    public async Task GetByNodeIdAsync_ReturnsWhatRepositoryReturns()
    {
        var nodeId = Guid.NewGuid();
        var expected = new List<KnowledgeRelation> { new() { Id = Guid.NewGuid(), SourceNodeId = nodeId } };
        _repository.Setup(r => r.GetByNodeIdAsync(nodeId)).ReturnsAsync(expected);

        var found = await _service.GetByNodeIdAsync(nodeId);

        Assert.That(found, Is.SameAs(expected));
    }

    [Test]
    public async Task DeleteAsync_WhenExists_RemovesAndReturnsTrue()
    {
        var knowledgeRelation = new KnowledgeRelation { Id = Guid.NewGuid() };
        _repository.Setup(r => r.GetByIdAsync(knowledgeRelation.Id)).ReturnsAsync(knowledgeRelation);

        var result = await _service.DeleteAsync(knowledgeRelation.Id);

        Assert.That(result, Is.True);
        _repository.Verify(r => r.Remove(knowledgeRelation), Times.Once);
        _repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_WhenNotFound_ReturnsFalse()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((KnowledgeRelation?)null);

        var result = await _service.DeleteAsync(Guid.NewGuid());

        Assert.That(result, Is.False);
    }
}
