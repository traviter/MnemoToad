using Moq;
using MnemoToad.Api.Services;
using MnemoToad.Data.Entities;
using MnemoToad.Data.Repositories;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Tests.Services;

[TestFixture]
public class KnowledgeNodeServiceTests
{
    private Mock<IKnowledgeNodeRepository> _repository = null!;
    private KnowledgeNodeService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new Mock<IKnowledgeNodeRepository>();
        _service = new KnowledgeNodeService(_repository.Object);
    }

    [Test]
    public async Task CreateAsync_WithValidData_ReturnsCreatedKnowledgeNode()
    {
        var nodeTypeId = Guid.NewGuid();

        var created = await _service.CreateAsync(new KnowledgeNode { NodeTypeId = nodeTypeId, CanonicalName = "France", Description = "A country" });

        Assert.That(created.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(created.NodeTypeId, Is.EqualTo(nodeTypeId));
        Assert.That(created.CanonicalName, Is.EqualTo("France"));
        Assert.That(created.Description, Is.EqualTo("A country"));
        _repository.Verify(r => r.AddAsync(It.Is<KnowledgeNode>(n => n.CanonicalName == "France")), Times.Once);
        _repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void CreateAsync_WithEmptyNodeTypeId_ThrowsValidationException()
    {
        Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(new KnowledgeNode { NodeTypeId = Guid.Empty, CanonicalName = "France" }));
        _repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Test]
    public void CreateAsync_WithBlankCanonicalName_ThrowsValidationException()
    {
        Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(new KnowledgeNode { NodeTypeId = Guid.NewGuid(), CanonicalName = "  " }));
        _repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    // Constraint-violation translation (e.g. duplicate NodeTypeId+CanonicalName, or a NodeTypeId
    // that doesn't exist) now happens in KnowledgeNodeRepository.SaveChangesAsync(), not here — the
    // service has nothing left to translate, it just needs to not swallow whatever the repository
    // throws.
    [Test]
    public void CreateAsync_WhenRepositoryThrowsValidationException_PropagatesException()
    {
        _repository.Setup(r => r.SaveChangesAsync())
            .ThrowsAsync(new ValidationException("A KnowledgeNode with the same NodeType and CanonicalName already exists."));

        Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(new KnowledgeNode { NodeTypeId = Guid.NewGuid(), CanonicalName = "France" }));
    }

    [Test]
    public void CreateAsync_WhenRepositoryThrowsUnrelatedException_PropagatesException()
    {
        _repository.Setup(r => r.SaveChangesAsync())
            .ThrowsAsync(new InvalidOperationException("could not connect to server"));

        Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(new KnowledgeNode { NodeTypeId = Guid.NewGuid(), CanonicalName = "France" }));
    }

    [Test]
    public async Task GetAllAsync_ReturnsWhatRepositoryReturns()
    {
        var expected = new List<KnowledgeNode> { new() { Id = Guid.NewGuid(), CanonicalName = "Paris" } };
        _repository.Setup(r => r.GetAllAsync(null)).ReturnsAsync(expected);

        var all = await _service.GetAllAsync();

        Assert.That(all, Is.SameAs(expected));
    }

    [Test]
    public async Task GetAllAsync_WithNodeTypeId_PassesThroughToRepository()
    {
        var nodeTypeId = Guid.NewGuid();
        var expected = new List<KnowledgeNode> { new() { Id = Guid.NewGuid(), NodeTypeId = nodeTypeId, CanonicalName = "Paris" } };
        _repository.Setup(r => r.GetAllAsync(nodeTypeId)).ReturnsAsync(expected);

        var all = await _service.GetAllAsync(nodeTypeId);

        Assert.That(all, Is.SameAs(expected));
    }

    [Test]
    public async Task GetByIdAsync_WhenExists_ReturnsKnowledgeNode()
    {
        var knowledgeNode = new KnowledgeNode { Id = Guid.NewGuid(), CanonicalName = "Paris" };
        _repository.Setup(r => r.GetByIdAsync(knowledgeNode.Id)).ReturnsAsync(knowledgeNode);

        var found = await _service.GetByIdAsync(knowledgeNode.Id);

        Assert.That(found, Is.SameAs(knowledgeNode));
    }

    [Test]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((KnowledgeNode?)null);

        var found = await _service.GetByIdAsync(Guid.NewGuid());

        Assert.That(found, Is.Null);
    }

    [Test]
    public async Task UpdateAsync_WhenNotFound_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((KnowledgeNode?)null);

        var updated = await _service.UpdateAsync(new KnowledgeNode { Id = Guid.NewGuid(), NodeTypeId = Guid.NewGuid(), CanonicalName = "Paris" });

        Assert.That(updated, Is.Null);
    }

    [Test]
    public void UpdateAsync_WithBlankCanonicalName_ThrowsValidationException()
    {
        var knowledgeNode = new KnowledgeNode { Id = Guid.NewGuid(), NodeTypeId = Guid.NewGuid(), CanonicalName = "Paris" };
        _repository.Setup(r => r.GetByIdAsync(knowledgeNode.Id)).ReturnsAsync(knowledgeNode);

        Assert.ThrowsAsync<ValidationException>(() => _service.UpdateAsync(new KnowledgeNode { Id = knowledgeNode.Id, NodeTypeId = knowledgeNode.NodeTypeId, CanonicalName = " " }));
        _repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Test]
    public async Task UpdateAsync_WithValidData_Succeeds()
    {
        var nodeType = new NodeType { Id = Guid.NewGuid(), Name = "Place" };
        var knowledgeNode = new KnowledgeNode { Id = Guid.NewGuid(), NodeTypeId = nodeType.Id, CanonicalName = "Paris", Description = "Old" };
        _repository.Setup(r => r.GetByIdAsync(knowledgeNode.Id)).ReturnsAsync(knowledgeNode);

        var updated = await _service.UpdateAsync(new KnowledgeNode { Id = knowledgeNode.Id, NodeTypeId = nodeType.Id, CanonicalName = "Paris", Description = "New description" });

        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Description, Is.EqualTo("New description"));
        _repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void UpdateAsync_WhenRepositoryThrowsValidationException_PropagatesException()
    {
        var knowledgeNode = new KnowledgeNode { Id = Guid.NewGuid(), NodeTypeId = Guid.NewGuid(), CanonicalName = "Rome" };
        _repository.Setup(r => r.GetByIdAsync(knowledgeNode.Id)).ReturnsAsync(knowledgeNode);
        _repository.Setup(r => r.SaveChangesAsync())
            .ThrowsAsync(new ValidationException("The specified NodeType does not exist."));

        Assert.ThrowsAsync<ValidationException>(() => _service.UpdateAsync(new KnowledgeNode { Id = knowledgeNode.Id, NodeTypeId = knowledgeNode.NodeTypeId, CanonicalName = "Paris" }));
    }

    [Test]
    public void UpdateAsync_WhenRepositoryThrowsUnrelatedException_PropagatesException()
    {
        var knowledgeNode = new KnowledgeNode { Id = Guid.NewGuid(), NodeTypeId = Guid.NewGuid(), CanonicalName = "Rome" };
        _repository.Setup(r => r.GetByIdAsync(knowledgeNode.Id)).ReturnsAsync(knowledgeNode);
        _repository.Setup(r => r.SaveChangesAsync())
            .ThrowsAsync(new InvalidOperationException("could not connect to server"));

        Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(new KnowledgeNode { Id = knowledgeNode.Id, NodeTypeId = knowledgeNode.NodeTypeId, CanonicalName = "Paris" }));
    }

    [Test]
    public async Task DeleteAsync_WhenExists_RemovesAndReturnsTrue()
    {
        var knowledgeNode = new KnowledgeNode { Id = Guid.NewGuid(), CanonicalName = "Paris" };
        _repository.Setup(r => r.GetByIdAsync(knowledgeNode.Id)).ReturnsAsync(knowledgeNode);

        var result = await _service.DeleteAsync(knowledgeNode.Id);

        Assert.That(result, Is.True);
        _repository.Verify(r => r.Remove(knowledgeNode), Times.Once);
        _repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_WhenNotFound_ReturnsFalse()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((KnowledgeNode?)null);

        var result = await _service.DeleteAsync(Guid.NewGuid());

        Assert.That(result, Is.False);
    }
}
