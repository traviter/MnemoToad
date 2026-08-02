using Microsoft.EntityFrameworkCore;
using Moq;
using MnemoToad.Api.Services;
using MnemoToad.Data.Entities;
using MnemoToad.Data.Repositories;
using Npgsql;
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

    private static PostgresException PostgresErrorWithCode(string sqlState) =>
        new(messageText: "simulated failure", severity: "ERROR", invariantSeverity: "ERROR", sqlState: sqlState);

    [Test]
    public async Task CreateAsync_WithValidData_ReturnsCreatedKnowledgeNode()
    {
        var nodeTypeId = Guid.NewGuid();

        var created = await _service.CreateAsync(nodeTypeId, "France", "A country");

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
        Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(Guid.Empty, "France", null));
        _repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Test]
    public void CreateAsync_WithBlankCanonicalName_ThrowsValidationException()
    {
        Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(Guid.NewGuid(), "  ", null));
        _repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Test]
    public void CreateAsync_WhenRepositoryThrowsUniqueViolation_ThrowsValidationException()
    {
        _repository.Setup(r => r.SaveChangesAsync())
            .ThrowsAsync(new DbUpdateException("save failed", PostgresErrorWithCode(PostgresErrorCodes.UniqueViolation)));

        Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(Guid.NewGuid(), "France", null));
    }

    [Test]
    public void CreateAsync_WhenRepositoryThrowsForeignKeyViolation_ThrowsValidationException()
    {
        _repository.Setup(r => r.SaveChangesAsync())
            .ThrowsAsync(new DbUpdateException("save failed", PostgresErrorWithCode(PostgresErrorCodes.ForeignKeyViolation)));

        Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(Guid.NewGuid(), "France", null));
    }

    [Test]
    public void CreateAsync_WhenRepositoryThrowsUnrelatedDbUpdateException_PropagatesException()
    {
        _repository.Setup(r => r.SaveChangesAsync())
            .ThrowsAsync(new DbUpdateException("save failed", PostgresErrorWithCode(PostgresErrorCodes.NotNullViolation)));

        Assert.ThrowsAsync<DbUpdateException>(() => _service.CreateAsync(Guid.NewGuid(), "France", null));
    }

    [Test]
    public void CreateAsync_WhenRepositoryThrowsConnectionFailure_PropagatesException()
    {
        _repository.Setup(r => r.SaveChangesAsync())
            .ThrowsAsync(new NpgsqlException("could not connect to server"));

        Assert.ThrowsAsync<NpgsqlException>(() => _service.CreateAsync(Guid.NewGuid(), "France", null));
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

        var updated = await _service.UpdateAsync(Guid.NewGuid(), Guid.NewGuid(), "Paris", null);

        Assert.That(updated, Is.Null);
    }

    [Test]
    public void UpdateAsync_WithBlankCanonicalName_ThrowsValidationException()
    {
        var knowledgeNode = new KnowledgeNode { Id = Guid.NewGuid(), NodeTypeId = Guid.NewGuid(), CanonicalName = "Paris" };
        _repository.Setup(r => r.GetByIdAsync(knowledgeNode.Id)).ReturnsAsync(knowledgeNode);

        Assert.ThrowsAsync<ValidationException>(() => _service.UpdateAsync(knowledgeNode.Id, knowledgeNode.NodeTypeId, " ", null));
        _repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Test]
    public async Task UpdateAsync_WithValidData_Succeeds()
    {
        var nodeType = new NodeType { Id = Guid.NewGuid(), Name = "Place" };
        var knowledgeNode = new KnowledgeNode { Id = Guid.NewGuid(), NodeTypeId = nodeType.Id, CanonicalName = "Paris", Description = "Old" };
        _repository.Setup(r => r.GetByIdAsync(knowledgeNode.Id)).ReturnsAsync(knowledgeNode);

        var updated = await _service.UpdateAsync(knowledgeNode.Id, nodeType.Id, "Paris", "New description");

        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Description, Is.EqualTo("New description"));
        _repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void UpdateAsync_WhenRepositoryThrowsUniqueViolation_ThrowsValidationException()
    {
        var knowledgeNode = new KnowledgeNode { Id = Guid.NewGuid(), NodeTypeId = Guid.NewGuid(), CanonicalName = "Rome" };
        _repository.Setup(r => r.GetByIdAsync(knowledgeNode.Id)).ReturnsAsync(knowledgeNode);
        _repository.Setup(r => r.SaveChangesAsync())
            .ThrowsAsync(new DbUpdateException("save failed", PostgresErrorWithCode(PostgresErrorCodes.UniqueViolation)));

        Assert.ThrowsAsync<ValidationException>(() => _service.UpdateAsync(knowledgeNode.Id, knowledgeNode.NodeTypeId, "Paris", null));
    }

    [Test]
    public void UpdateAsync_WhenRepositoryThrowsForeignKeyViolation_ThrowsValidationException()
    {
        var knowledgeNode = new KnowledgeNode { Id = Guid.NewGuid(), NodeTypeId = Guid.NewGuid(), CanonicalName = "Rome" };
        _repository.Setup(r => r.GetByIdAsync(knowledgeNode.Id)).ReturnsAsync(knowledgeNode);
        _repository.Setup(r => r.SaveChangesAsync())
            .ThrowsAsync(new DbUpdateException("save failed", PostgresErrorWithCode(PostgresErrorCodes.ForeignKeyViolation)));

        Assert.ThrowsAsync<ValidationException>(() => _service.UpdateAsync(knowledgeNode.Id, knowledgeNode.NodeTypeId, "Paris", null));
    }

    [Test]
    public void UpdateAsync_WhenRepositoryThrowsConnectionFailure_PropagatesException()
    {
        var knowledgeNode = new KnowledgeNode { Id = Guid.NewGuid(), NodeTypeId = Guid.NewGuid(), CanonicalName = "Rome" };
        _repository.Setup(r => r.GetByIdAsync(knowledgeNode.Id)).ReturnsAsync(knowledgeNode);
        _repository.Setup(r => r.SaveChangesAsync())
            .ThrowsAsync(new NpgsqlException("could not connect to server"));

        Assert.ThrowsAsync<NpgsqlException>(() => _service.UpdateAsync(knowledgeNode.Id, knowledgeNode.NodeTypeId, "Paris", null));
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
