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
    private Mock<IKnowledgeNodeRepository> _knowledgeNodeRepository = null!;
    private NodeTypeService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new Mock<INodeTypeRepository>();
        _knowledgeNodeRepository = new Mock<IKnowledgeNodeRepository>();
        _service = new NodeTypeService(_repository.Object, _knowledgeNodeRepository.Object);
    }

    [Test]
    public async Task CreateAsync_WithValidName_ReturnsCreatedNodeType()
    {
        _repository.Setup(r => r.ExistsWithNameAsync("Person", null)).ReturnsAsync(false);

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
    }

    [Test]
    public void CreateAsync_WithDuplicateName_ThrowsValidationException()
    {
        _repository.Setup(r => r.ExistsWithNameAsync("Person", null)).ReturnsAsync(true);

        Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(new NodeType { Name = "Person" }));
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
    }

    [Test]
    public void UpdateAsync_WithAnotherNodeTypesName_ThrowsValidationException()
    {
        var nodeType = new NodeType { Id = Guid.NewGuid(), Name = "Place" };
        _repository.Setup(r => r.GetByIdAsync(nodeType.Id)).ReturnsAsync(nodeType);
        _repository.Setup(r => r.ExistsWithNameAsync("Person", nodeType.Id)).ReturnsAsync(true);

        Assert.ThrowsAsync<ValidationException>(() => _service.UpdateAsync(new NodeType { Id = nodeType.Id, Name = "Person" }));
    }

    [Test]
    public async Task UpdateAsync_WithOwnUnchangedName_Succeeds()
    {
        var nodeType = new NodeType { Id = Guid.NewGuid(), Name = "Person", Description = "Old" };
        _repository.Setup(r => r.GetByIdAsync(nodeType.Id)).ReturnsAsync(nodeType);
        _repository.Setup(r => r.ExistsWithNameAsync("Person", nodeType.Id)).ReturnsAsync(false);

        var updated = await _service.UpdateAsync(new NodeType { Id = nodeType.Id, Name = "Person", Description = "New description" });

        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Description, Is.EqualTo("New description"));
    }

    [Test]
    public async Task DeleteAsync_WhenExists_RemovesAndReturnsTrue()
    {
        var nodeType = new NodeType { Id = Guid.NewGuid(), Name = "Person" };
        _repository.Setup(r => r.GetByIdAsync(nodeType.Id)).ReturnsAsync(nodeType);
        _knowledgeNodeRepository.Setup(r => r.ExistsByNodeTypeIdAsync(nodeType.Id)).ReturnsAsync(false);

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
    public void DeleteAsync_WhenReferencedByKnowledgeNode_ThrowsValidationException()
    {
        var nodeType = new NodeType { Id = Guid.NewGuid(), Name = "Person" };
        _repository.Setup(r => r.GetByIdAsync(nodeType.Id)).ReturnsAsync(nodeType);
        _knowledgeNodeRepository.Setup(r => r.ExistsByNodeTypeIdAsync(nodeType.Id)).ReturnsAsync(true);

        Assert.ThrowsAsync<ValidationException>(() => _service.DeleteAsync(nodeType.Id));
        _repository.Verify(r => r.Remove(It.IsAny<NodeType>()), Times.Never);
    }
}
