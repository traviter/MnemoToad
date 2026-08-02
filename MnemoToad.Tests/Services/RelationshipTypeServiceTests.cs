using Moq;
using MnemoToad.Api.Services;
using MnemoToad.Data.Entities;
using MnemoToad.Data.Repositories;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Tests.Services;

[TestFixture]
public class RelationshipTypeServiceTests
{
    private Mock<IRelationshipTypeRepository> _repository = null!;
    private RelationshipTypeService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new Mock<IRelationshipTypeRepository>();
        _service = new RelationshipTypeService(_repository.Object);
    }

    [Test]
    public async Task CreateAsync_WithValidData_ReturnsCreatedRelationshipType()
    {
        var created = await _service.CreateAsync(new RelationshipType { Name = "hasCapital", InverseName = "isCapitalOf", Description = "Links a country to its capital" });

        Assert.That(created.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(created.Name, Is.EqualTo("hasCapital"));
        Assert.That(created.InverseName, Is.EqualTo("isCapitalOf"));
        Assert.That(created.Description, Is.EqualTo("Links a country to its capital"));
        _repository.Verify(r => r.AddAsync(It.Is<RelationshipType>(t => t.Name == "hasCapital")), Times.Once);
        _repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void CreateAsync_WithBlankName_ThrowsValidationException()
    {
        Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(new RelationshipType { Name = "  " }));
        _repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    // Constraint-violation translation (e.g. duplicate Name) now happens in
    // RelationshipTypeRepository.SaveChangesAsync(), not here — the service has nothing left to
    // translate, it just needs to not swallow whatever the repository throws.
    [Test]
    public void CreateAsync_WhenRepositoryThrowsValidationException_PropagatesException()
    {
        _repository.Setup(r => r.SaveChangesAsync())
            .ThrowsAsync(new ValidationException("A RelationshipType with that name already exists."));

        Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(new RelationshipType { Name = "hasCapital" }));
    }

    [Test]
    public void CreateAsync_WhenRepositoryThrowsUnrelatedException_PropagatesException()
    {
        _repository.Setup(r => r.SaveChangesAsync())
            .ThrowsAsync(new InvalidOperationException("could not connect to server"));

        Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(new RelationshipType { Name = "hasCapital" }));
    }

    [Test]
    public async Task GetAllAsync_ReturnsWhatRepositoryReturns()
    {
        var expected = new List<RelationshipType> { new() { Id = Guid.NewGuid(), Name = "locatedIn" } };
        _repository.Setup(r => r.GetAllAsync()).ReturnsAsync(expected);

        var all = await _service.GetAllAsync();

        Assert.That(all, Is.SameAs(expected));
    }

    [Test]
    public async Task GetByIdAsync_WhenExists_ReturnsRelationshipType()
    {
        var relationshipType = new RelationshipType { Id = Guid.NewGuid(), Name = "locatedIn" };
        _repository.Setup(r => r.GetByIdAsync(relationshipType.Id)).ReturnsAsync(relationshipType);

        var found = await _service.GetByIdAsync(relationshipType.Id);

        Assert.That(found, Is.SameAs(relationshipType));
    }

    [Test]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((RelationshipType?)null);

        var found = await _service.GetByIdAsync(Guid.NewGuid());

        Assert.That(found, Is.Null);
    }

    [Test]
    public async Task UpdateAsync_WhenNotFound_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((RelationshipType?)null);

        var updated = await _service.UpdateAsync(new RelationshipType { Id = Guid.NewGuid(), Name = "locatedIn" });

        Assert.That(updated, Is.Null);
    }

    [Test]
    public void UpdateAsync_WithBlankName_ThrowsValidationException()
    {
        var relationshipType = new RelationshipType { Id = Guid.NewGuid(), Name = "locatedIn" };
        _repository.Setup(r => r.GetByIdAsync(relationshipType.Id)).ReturnsAsync(relationshipType);

        Assert.ThrowsAsync<ValidationException>(() => _service.UpdateAsync(new RelationshipType { Id = relationshipType.Id, Name = " " }));
        _repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Test]
    public async Task UpdateAsync_WithValidData_Succeeds()
    {
        var relationshipType = new RelationshipType { Id = Guid.NewGuid(), Name = "locatedIn", Description = "Old" };
        _repository.Setup(r => r.GetByIdAsync(relationshipType.Id)).ReturnsAsync(relationshipType);

        var updated = await _service.UpdateAsync(new RelationshipType { Id = relationshipType.Id, Name = "locatedIn", InverseName = "contains", Description = "New description" });

        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.InverseName, Is.EqualTo("contains"));
        Assert.That(updated.Description, Is.EqualTo("New description"));
        _repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void UpdateAsync_WhenRepositoryThrowsValidationException_PropagatesException()
    {
        var relationshipType = new RelationshipType { Id = Guid.NewGuid(), Name = "locatedIn" };
        _repository.Setup(r => r.GetByIdAsync(relationshipType.Id)).ReturnsAsync(relationshipType);
        _repository.Setup(r => r.SaveChangesAsync())
            .ThrowsAsync(new ValidationException("A RelationshipType with that name already exists."));

        Assert.ThrowsAsync<ValidationException>(() => _service.UpdateAsync(new RelationshipType { Id = relationshipType.Id, Name = "hasCapital" }));
    }

    [Test]
    public void UpdateAsync_WhenRepositoryThrowsUnrelatedException_PropagatesException()
    {
        var relationshipType = new RelationshipType { Id = Guid.NewGuid(), Name = "locatedIn" };
        _repository.Setup(r => r.GetByIdAsync(relationshipType.Id)).ReturnsAsync(relationshipType);
        _repository.Setup(r => r.SaveChangesAsync())
            .ThrowsAsync(new InvalidOperationException("could not connect to server"));

        Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(new RelationshipType { Id = relationshipType.Id, Name = "hasCapital" }));
    }

    [Test]
    public async Task DeleteAsync_WhenExists_RemovesAndReturnsTrue()
    {
        var relationshipType = new RelationshipType { Id = Guid.NewGuid(), Name = "locatedIn" };
        _repository.Setup(r => r.GetByIdAsync(relationshipType.Id)).ReturnsAsync(relationshipType);

        var result = await _service.DeleteAsync(relationshipType.Id);

        Assert.That(result, Is.True);
        _repository.Verify(r => r.Remove(relationshipType), Times.Once);
        _repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_WhenNotFound_ReturnsFalse()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((RelationshipType?)null);

        var result = await _service.DeleteAsync(Guid.NewGuid());

        Assert.That(result, Is.False);
    }
}
