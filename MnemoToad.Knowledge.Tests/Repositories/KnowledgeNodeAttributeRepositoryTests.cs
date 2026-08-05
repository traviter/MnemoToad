using Microsoft.EntityFrameworkCore;
using MnemoToad.Knowledge.Data.Entities;
using MnemoToad.Knowledge.Data.Repositories;
using MnemoToad.Knowledge.Tests.TestSupport;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Knowledge.Tests.Repositories;

[TestFixture]
public class KnowledgeNodeAttributeRepositoryTests
{
    private MockableAppDbContext _db = null!;
    private KnowledgeNodeAttributeRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        _db = new MockableAppDbContext();
        _repository = new KnowledgeNodeAttributeRepository(_db);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    [Test]
    public async Task GetByNodeIdAsync_ReturnsOnlyAttributesForThatNode()
    {
        var knowledgeNodeId = Guid.NewGuid();
        await _db.KnowledgeNodeAttribute.AddRangeAsync(
            new KnowledgeNodeAttribute { Id = Guid.NewGuid(), KnowledgeNodeId = knowledgeNodeId, AttributeTypeId = Guid.NewGuid(), Value = "FR" },
            new KnowledgeNodeAttribute { Id = Guid.NewGuid(), KnowledgeNodeId = Guid.NewGuid(), AttributeTypeId = Guid.NewGuid(), Value = "DE" });
        await _db.SaveChangesAsync();

        var found = await _repository.GetByNodeIdAsync(knowledgeNodeId);

        Assert.That(found, Has.Count.EqualTo(1));
        Assert.That(found[0].Value, Is.EqualTo("FR"));
    }

    [Test]
    public async Task CreateAsync_PersistsAndReturnsKnowledgeNodeAttribute()
    {
        var knowledgeNodeAttribute = new KnowledgeNodeAttribute { Id = Guid.NewGuid(), KnowledgeNodeId = Guid.NewGuid(), AttributeTypeId = Guid.NewGuid(), Value = "FR" };

        var created = await _repository.CreateAsync(knowledgeNodeAttribute);

        Assert.That(created, Is.SameAs(knowledgeNodeAttribute));
        Assert.That(await _db.KnowledgeNodeAttribute.FindAsync(knowledgeNodeAttribute.Id), Is.Not.Null);
    }

    [Test]
    public async Task UpdateAsync_WhenNotFound_ReturnsNull()
    {
        var updated = await _repository.UpdateAsync(new KnowledgeNodeAttribute { Id = Guid.NewGuid(), KnowledgeNodeId = Guid.NewGuid(), AttributeTypeId = Guid.NewGuid(), Value = "FR" });

        Assert.That(updated, Is.Null);
    }

    [Test]
    public async Task UpdateAsync_WithValidData_UpdatesAndReturnsKnowledgeNodeAttribute()
    {
        var knowledgeNodeAttribute = new KnowledgeNodeAttribute { Id = Guid.NewGuid(), KnowledgeNodeId = Guid.NewGuid(), AttributeTypeId = Guid.NewGuid(), Value = "FR" };
        await _db.KnowledgeNodeAttribute.AddAsync(knowledgeNodeAttribute);
        await _db.SaveChangesAsync();

        var updated = await _repository.UpdateAsync(new KnowledgeNodeAttribute
        {
            Id = knowledgeNodeAttribute.Id,
            KnowledgeNodeId = knowledgeNodeAttribute.KnowledgeNodeId,
            AttributeTypeId = knowledgeNodeAttribute.AttributeTypeId,
            Value = "68000000"
        });

        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Value, Is.EqualTo("68000000"));
    }

    [Test]
    public async Task DeleteAsync_WhenExists_RemovesKnowledgeNodeAttributeAndReturnsTrue()
    {
        var knowledgeNodeAttribute = new KnowledgeNodeAttribute { Id = Guid.NewGuid(), KnowledgeNodeId = Guid.NewGuid(), AttributeTypeId = Guid.NewGuid(), Value = "FR" };
        await _db.KnowledgeNodeAttribute.AddAsync(knowledgeNodeAttribute);
        await _db.SaveChangesAsync();

        var result = await _repository.DeleteAsync(knowledgeNodeAttribute.Id);

        Assert.That(result, Is.True);
        Assert.That(await _db.KnowledgeNodeAttribute.AsNoTracking().FirstOrDefaultAsync(a => a.Id == knowledgeNodeAttribute.Id), Is.Null);
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

        var ex = Assert.ThrowsAsync<ValidationException>(() => _repository.CreateAsync(new KnowledgeNodeAttribute
        {
            Id = Guid.NewGuid(),
            KnowledgeNodeId = Guid.NewGuid(),
            AttributeTypeId = Guid.NewGuid(),
            Value = "FR"
        }));

        Assert.That(ex!.Message, Is.EqualTo("An attribute of that type already exists for this KnowledgeNode."));
    }

    [Test]
    public void CreateAsync_OnKnowledgeNodeForeignKeyViolation_ThrowsValidationExceptionAboutKnowledgeNode()
    {
        _db.ThrowOnSaveChanges(PostgresExceptionFactory.ForeignKeyViolation(constraintName: "fk_knowledge_node_attribute_knowledge_node_id"));

        var ex = Assert.ThrowsAsync<ValidationException>(() => _repository.CreateAsync(new KnowledgeNodeAttribute
        {
            Id = Guid.NewGuid(),
            KnowledgeNodeId = Guid.NewGuid(),
            AttributeTypeId = Guid.NewGuid(),
            Value = "FR"
        }));

        Assert.That(ex!.Message, Is.EqualTo("The specified KnowledgeNode does not exist."));
    }

    [Test]
    public void CreateAsync_OnAttributeTypeForeignKeyViolation_ThrowsValidationExceptionAboutAttributeType()
    {
        _db.ThrowOnSaveChanges(PostgresExceptionFactory.ForeignKeyViolation(constraintName: "fk_knowledge_node_attribute_attribute_type_id"));

        var ex = Assert.ThrowsAsync<ValidationException>(() => _repository.CreateAsync(new KnowledgeNodeAttribute
        {
            Id = Guid.NewGuid(),
            KnowledgeNodeId = Guid.NewGuid(),
            AttributeTypeId = Guid.NewGuid(),
            Value = "FR"
        }));

        Assert.That(ex!.Message, Is.EqualTo("The specified AttributeType does not exist."));
    }
}
