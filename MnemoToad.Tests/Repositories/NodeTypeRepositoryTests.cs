using MnemoToad.Data.Entities;
using MnemoToad.Data.Repositories;
using MnemoToad.Tests.TestSupport;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Tests.Repositories;

[TestFixture]
public class NodeTypeRepositoryTests
{
    private MockableAppDbContext _db = null!;
    private NodeTypeRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        _db = new MockableAppDbContext();
        _repository = new NodeTypeRepository(_db);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    [Test]
    public async Task GetAllAsync_ReturnsNodeTypesOrderedByName()
    {
        await _db.NodeType.AddRangeAsync(
            new NodeType { Id = Guid.NewGuid(), Name = "Place" },
            new NodeType { Id = Guid.NewGuid(), Name = "Animal" });
        await _db.SaveChangesAsync();

        var all = await _repository.GetAllAsync();

        Assert.That(all.Select(n => n.Name), Is.EqualTo(new[] { "Animal", "Place" }));
    }

    [Test]
    public async Task GetByIdAsync_WhenExists_ReturnsNodeType()
    {
        var nodeType = new NodeType { Id = Guid.NewGuid(), Name = "Person" };
        await _db.NodeType.AddAsync(nodeType);
        await _db.SaveChangesAsync();

        var found = await _repository.GetByIdAsync(nodeType.Id);

        Assert.That(found?.Name, Is.EqualTo("Person"));
    }

    [Test]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        var found = await _repository.GetByIdAsync(Guid.NewGuid());

        Assert.That(found, Is.Null);
    }

    [Test]
    public async Task AddAsync_ThenSaveChangesAsync_PersistsNodeType()
    {
        var nodeType = new NodeType { Id = Guid.NewGuid(), Name = "Person" };

        await _repository.AddAsync(nodeType);
        await _repository.SaveChangesAsync();

        Assert.That(await _db.NodeType.FindAsync(nodeType.Id), Is.Not.Null);
    }

    [Test]
    public async Task Remove_ThenSaveChangesAsync_DeletesNodeType()
    {
        var nodeType = new NodeType { Id = Guid.NewGuid(), Name = "Person" };
        await _db.NodeType.AddAsync(nodeType);
        await _db.SaveChangesAsync();

        _repository.Remove(nodeType);
        await _repository.SaveChangesAsync();

        Assert.That(await _db.NodeType.FindAsync(nodeType.Id), Is.Null);
    }

    [Test]
    public async Task SaveChangesAsync_OnUniqueViolation_ThrowsValidationExceptionWithDuplicateNameMessage()
    {
        _db.ThrowOnSaveChanges(PostgresExceptionFactory.UniqueViolation());
        await _repository.AddAsync(new NodeType { Id = Guid.NewGuid(), Name = "Person" });

        var ex = Assert.ThrowsAsync<ValidationException>(() => _repository.SaveChangesAsync());

        Assert.That(ex!.Message, Is.EqualTo("A NodeType with that name already exists."));
    }

    [Test]
    public void SaveChangesAsync_OnForeignKeyViolation_ThrowsValidationExceptionWithReferencedMessage()
    {
        _db.ThrowOnSaveChanges(PostgresExceptionFactory.ForeignKeyViolation());
        _repository.Remove(new NodeType { Id = Guid.NewGuid(), Name = "Person" });

        var ex = Assert.ThrowsAsync<ValidationException>(() => _repository.SaveChangesAsync());

        Assert.That(ex!.Message, Is.EqualTo("The NodeType cannot be deleted because it is referenced by one or more KnowledgeNodes."));
    }
}
