using Moq;
using MnemoToad.Api.Services;
using MnemoToad.Data.Entities;
using MnemoToad.Data.Repositories;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Tests.Services;

[TestFixture]
public class NodeTypeServiceTests
{
    private Mock<INodeTypeRepository> _repository = null!;
    private NodeTypeService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new Mock<INodeTypeRepository>();
        _service = new NodeTypeService(_repository.Object);
    }

    [Test]
    public async Task CreateAsync_WithValidName_ReturnsCreatedNodeType()
    {
        var created = await _service.CreateAsync(new NodeType { Name = "Person", Description = "A human being" });

        Assert.That(created.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(created.Name, Is.EqualTo("Person"));
        Assert.That(created.Description, Is.EqualTo("A human being"));
        _repository.Verify(r => r.AddAsync(It.Is<NodeType>(n => n.Name == "Person")), Times.Once);
        _repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void CreateAsync_WithBlankName_ThrowsValidationException()
    {
        Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(new NodeType { Name = "  " }));
        _repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    // Constraint-violation translation (e.g. duplicate Name) now happens in
    // NodeTypeRepository.SaveChangesAsync(), not here — the service has nothing left to translate,
    // it just needs to not swallow whatever the repository throws.
    [Test]
    public void CreateAsync_WhenRepositoryThrowsValidationException_PropagatesException()
    {
        _repository.Setup(r => r.SaveChangesAsync())
            .ThrowsAsync(new ValidationException("A NodeType with that name already exists."));

        Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(new NodeType { Name = "Person" }));
    }

    [Test]
    public void CreateAsync_WhenRepositoryThrowsUnrelatedException_PropagatesException()
    {
        _repository.Setup(r => r.SaveChangesAsync())
            .ThrowsAsync(new InvalidOperationException("could not connect to server"));

        Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(new NodeType { Name = "Person" }));
    }

    [Test]
    public async Task GetAllAsync_ReturnsWhatRepositoryReturns()
    {
        var expected = new List<NodeType> { new() { Id = Guid.NewGuid(), Name = "Apple" } };
        _repository.Setup(r => r.GetAllAsync()).ReturnsAsync(expected);

        var all = await _service.GetAllAsync();

        Assert.That(all, Is.SameAs(expected));
    }

    [Test]
    public async Task GetByIdAsync_WhenExists_ReturnsNodeType()
    {
        var nodeType = new NodeType { Id = Guid.NewGuid(), Name = "Person" };
        _repository.Setup(r => r.GetByIdAsync(nodeType.Id)).ReturnsAsync(nodeType);

        var found = await _service.GetByIdAsync(nodeType.Id);

        Assert.That(found, Is.SameAs(nodeType));
    }

    [Test]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((NodeType?)null);

        var found = await _service.GetByIdAsync(Guid.NewGuid());

        Assert.That(found, Is.Null);
    }

    [Test]
    public async Task UpdateAsync_WhenNotFound_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((NodeType?)null);

        var updated = await _service.UpdateAsync(new NodeType { Id = Guid.NewGuid(), Name = "Person" });

        Assert.That(updated, Is.Null);
    }

    [Test]
    public void UpdateAsync_WithBlankName_ThrowsValidationException()
    {
        var nodeType = new NodeType { Id = Guid.NewGuid(), Name = "Person" };
        _repository.Setup(r => r.GetByIdAsync(nodeType.Id)).ReturnsAsync(nodeType);

        Assert.ThrowsAsync<ValidationException>(() => _service.UpdateAsync(new NodeType { Id = nodeType.Id, Name = " " }));
        _repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Test]
    public async Task UpdateAsync_WithValidData_Succeeds()
    {
        var nodeType = new NodeType { Id = Guid.NewGuid(), Name = "Person", Description = "Old" };
        _repository.Setup(r => r.GetByIdAsync(nodeType.Id)).ReturnsAsync(nodeType);

        var updated = await _service.UpdateAsync(new NodeType { Id = nodeType.Id, Name = "Person", Description = "New description" });

        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Description, Is.EqualTo("New description"));
        _repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void UpdateAsync_WhenRepositoryThrowsValidationException_PropagatesException()
    {
        var nodeType = new NodeType { Id = Guid.NewGuid(), Name = "Place" };
        _repository.Setup(r => r.GetByIdAsync(nodeType.Id)).ReturnsAsync(nodeType);
        _repository.Setup(r => r.SaveChangesAsync())
            .ThrowsAsync(new ValidationException("A NodeType with that name already exists."));

        Assert.ThrowsAsync<ValidationException>(() => _service.UpdateAsync(new NodeType { Id = nodeType.Id, Name = "Person" }));
    }

    [Test]
    public void UpdateAsync_WhenRepositoryThrowsUnrelatedException_PropagatesException()
    {
        var nodeType = new NodeType { Id = Guid.NewGuid(), Name = "Place" };
        _repository.Setup(r => r.GetByIdAsync(nodeType.Id)).ReturnsAsync(nodeType);
        _repository.Setup(r => r.SaveChangesAsync())
            .ThrowsAsync(new InvalidOperationException("could not connect to server"));

        Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(new NodeType { Id = nodeType.Id, Name = "Person" }));
    }

    [Test]
    public async Task DeleteAsync_WhenExists_RemovesAndReturnsTrue()
    {
        var nodeType = new NodeType { Id = Guid.NewGuid(), Name = "Person" };
        _repository.Setup(r => r.GetByIdAsync(nodeType.Id)).ReturnsAsync(nodeType);

        var result = await _service.DeleteAsync(nodeType.Id);

        Assert.That(result, Is.True);
        _repository.Verify(r => r.Remove(nodeType), Times.Once);
        _repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_WhenNotFound_ReturnsFalse()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((NodeType?)null);

        var result = await _service.DeleteAsync(Guid.NewGuid());

        Assert.That(result, Is.False);
    }

    [Test]
    public void DeleteAsync_WhenRepositoryThrowsValidationException_PropagatesException()
    {
        var nodeType = new NodeType { Id = Guid.NewGuid(), Name = "Person" };
        _repository.Setup(r => r.GetByIdAsync(nodeType.Id)).ReturnsAsync(nodeType);
        _repository.Setup(r => r.SaveChangesAsync())
            .ThrowsAsync(new ValidationException("The NodeType cannot be deleted because it is referenced by one or more KnowledgeNodes."));

        Assert.ThrowsAsync<ValidationException>(() => _service.DeleteAsync(nodeType.Id));
    }
}
