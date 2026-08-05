using Microsoft.EntityFrameworkCore;
using MnemoToad.Knowledge.Data.Entities;
using MnemoToad.Knowledge.Data.Repositories;
using MnemoToad.Knowledge.Tests.TestSupport;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Knowledge.Tests.Repositories;

[TestFixture]
public class AttributeTypeRepositoryTests
{
    private MockableAppDbContext _db = null!;
    private AttributeTypeRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        _db = new MockableAppDbContext();
        _repository = new AttributeTypeRepository(_db);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    [Test]
    public async Task GetAllAsync_ReturnsAttributeTypesOrderedByName()
    {
        await _db.AttributeType.AddRangeAsync(
            new AttributeType { Id = Guid.NewGuid(), Name = "Population" },
            new AttributeType { Id = Guid.NewGuid(), Name = "IsoCode" });
        await _db.SaveChangesAsync();

        var all = await _repository.GetAllAsync();

        Assert.That(all.Select(a => a.Name), Is.EqualTo(new[] { "IsoCode", "Population" }));
    }

    [Test]
    public async Task GetByIdAsync_WhenExists_ReturnsAttributeType()
    {
        var attributeType = new AttributeType { Id = Guid.NewGuid(), Name = "Population" };
        await _db.AttributeType.AddAsync(attributeType);
        await _db.SaveChangesAsync();

        var found = await _repository.GetByIdAsync(attributeType.Id);

        Assert.That(found?.Name, Is.EqualTo("Population"));
    }

    [Test]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        var found = await _repository.GetByIdAsync(Guid.NewGuid());

        Assert.That(found, Is.Null);
    }

    [Test]
    public async Task CreateAsync_PersistsAndReturnsAttributeType()
    {
        var attributeType = new AttributeType { Id = Guid.NewGuid(), Name = "Population" };

        var created = await _repository.CreateAsync(attributeType);

        Assert.That(created, Is.SameAs(attributeType));
        Assert.That(await _db.AttributeType.FindAsync(attributeType.Id), Is.Not.Null);
    }

    [Test]
    public async Task UpdateAsync_WhenNotFound_ReturnsNull()
    {
        var updated = await _repository.UpdateAsync(new AttributeType { Id = Guid.NewGuid(), Name = "Population" });

        Assert.That(updated, Is.Null);
    }

    [Test]
    public async Task UpdateAsync_WithValidData_UpdatesAndReturnsAttributeType()
    {
        var attributeType = new AttributeType { Id = Guid.NewGuid(), Name = "Population", Description = "Old" };
        await _db.AttributeType.AddAsync(attributeType);
        await _db.SaveChangesAsync();

        var updated = await _repository.UpdateAsync(new AttributeType { Id = attributeType.Id, Name = "Population", Description = "New description" });

        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Description, Is.EqualTo("New description"));
    }

    [Test]
    public async Task UpdateAsync_OnUniqueViolation_ThrowsValidationExceptionWithDuplicateNameMessage()
    {
        var attributeType = new AttributeType { Id = Guid.NewGuid(), Name = "Population" };
        await _db.AttributeType.AddAsync(attributeType);
        await _db.SaveChangesAsync();
        _db.ThrowOnSaveChanges(PostgresExceptionFactory.UniqueViolation());

        var ex = Assert.ThrowsAsync<ValidationException>(
            () => _repository.UpdateAsync(new AttributeType { Id = attributeType.Id, Name = "IsoCode" }));

        Assert.That(ex!.Message, Is.EqualTo("An AttributeType with that name already exists."));
    }

    [Test]
    public async Task DeleteAsync_WhenExists_RemovesAttributeTypeAndReturnsTrue()
    {
        var attributeType = new AttributeType { Id = Guid.NewGuid(), Name = "Population" };
        await _db.AttributeType.AddAsync(attributeType);
        await _db.SaveChangesAsync();

        var result = await _repository.DeleteAsync(attributeType.Id);

        Assert.That(result, Is.True);
        Assert.That(await _db.AttributeType.AsNoTracking().FirstOrDefaultAsync(a => a.Id == attributeType.Id), Is.Null);
    }

    [Test]
    public async Task DeleteAsync_WhenNotFound_ReturnsFalse()
    {
        var result = await _repository.DeleteAsync(Guid.NewGuid());

        Assert.That(result, Is.False);
    }

    [Test]
    public void CreateAsync_OnUniqueViolation_ThrowsValidationExceptionWithDuplicateNameMessage()
    {
        _db.ThrowOnSaveChanges(PostgresExceptionFactory.UniqueViolation());

        var ex = Assert.ThrowsAsync<ValidationException>(
            () => _repository.CreateAsync(new AttributeType { Id = Guid.NewGuid(), Name = "Population" }));

        Assert.That(ex!.Message, Is.EqualTo("An AttributeType with that name already exists."));
    }

    [Test]
    public async Task DeleteAsync_OnForeignKeyViolation_ThrowsValidationExceptionWithReferencedMessage()
    {
        var attributeType = new AttributeType { Id = Guid.NewGuid(), Name = "Population" };
        await _db.AttributeType.AddAsync(attributeType);
        await _db.SaveChangesAsync();
        _db.ThrowOnExecuteDelete<AttributeType>(PostgresExceptionFactory.ForeignKeyViolation());

        var ex = Assert.ThrowsAsync<ValidationException>(() => _repository.DeleteAsync(attributeType.Id));

        Assert.That(ex!.Message, Is.EqualTo("The AttributeType cannot be deleted because it is referenced by one or more KnowledgeNodeAttributes."));
    }
}
