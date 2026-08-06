using Microsoft.EntityFrameworkCore;
using MnemoToad.Knowledge.Data.Entities;
using MnemoToad.Knowledge.Data.Repositories;
using MnemoToad.Knowledge.Tests.TestSupport;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace MnemoToad.Knowledge.Tests.Repositories;

[TestFixture]
public class KnowledgeNodeRepositoryTests
{
    private MockableAppDbContext _db = null!;
    private KnowledgeNodeRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        _db = new MockableAppDbContext();
        _repository = new KnowledgeNodeRepository(_db);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    [Test]
    public async Task GetAllAsync_ReturnsKnowledgeNodesOrderedByCanonicalName()
    {
        await _db.KnowledgeNode.AddRangeAsync(
            new KnowledgeNode { Id = Guid.NewGuid(), CanonicalName = "Venus" },
            new KnowledgeNode { Id = Guid.NewGuid(), CanonicalName = "Mercury" });
        await _db.SaveChangesAsync();

        var all = await _repository.GetAllAsync();

        Assert.That(all.Select(n => n.CanonicalName), Is.EqualTo(new[] { "Mercury", "Venus" }));
    }

    [Test]
    public async Task GetAllAsync_WithNodeTypeIdFilter_ReturnsOnlyMatchingNodes()
    {
        var nodeTypeId = Guid.NewGuid();
        await _db.KnowledgeNode.AddRangeAsync(
            new KnowledgeNode { Id = Guid.NewGuid(), NodeTypeId = nodeTypeId, CanonicalName = "Mercury" },
            new KnowledgeNode { Id = Guid.NewGuid(), NodeTypeId = Guid.NewGuid(), CanonicalName = "Venus" });
        await _db.SaveChangesAsync();

        var all = await _repository.GetAllAsync(nodeTypeId);

        Assert.That(all.Select(n => n.CanonicalName), Is.EqualTo(new[] { "Mercury" }));
    }

    [Test]
    public async Task GetByIdAsync_WhenExists_ReturnsKnowledgeNode()
    {
        var knowledgeNode = new KnowledgeNode { Id = Guid.NewGuid(), CanonicalName = "Mercury" };
        await _db.KnowledgeNode.AddAsync(knowledgeNode);
        await _db.SaveChangesAsync();

        var found = await _repository.GetByIdAsync(knowledgeNode.Id);

        Assert.That(found?.CanonicalName, Is.EqualTo("Mercury"));
        Assert.That(found!.Attributes, Is.Empty);
    }

    [Test]
    public async Task GetByIdAsync_WhenExists_PopulatesAttributesKeyedByKey()
    {
        var knowledgeNode = new KnowledgeNode { Id = Guid.NewGuid(), CanonicalName = "France" };
        await _db.KnowledgeNode.AddAsync(knowledgeNode);
        await _db.KnowledgeNodeAttribute.AddAsync(new KnowledgeNodeAttribute
        {
            KnowledgeNodeId = knowledgeNode.Id,
            Key = "isoCode",
            Value = JsonValue.Create("FR")
        });
        await _db.SaveChangesAsync();

        var found = await _repository.GetByIdAsync(knowledgeNode.Id);

        Assert.That(found!.Attributes!.Keys, Is.EquivalentTo(new[] { "isoCode" }));
        Assert.That(found.Attributes!["isoCode"]!.GetValue<string>(), Is.EqualTo("FR"));
    }

    [Test]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        var found = await _repository.GetByIdAsync(Guid.NewGuid());

        Assert.That(found, Is.Null);
    }

    [Test]
    public async Task CreateAsync_PersistsAndReturnsKnowledgeNode()
    {
        var knowledgeNode = new KnowledgeNode { Id = Guid.NewGuid(), CanonicalName = "Mercury" };

        var created = await _repository.CreateAsync(knowledgeNode);

        Assert.That(created, Is.SameAs(knowledgeNode));
        Assert.That(await _db.KnowledgeNode.FindAsync(knowledgeNode.Id), Is.Not.Null);
    }

    [Test]
    public async Task CreateAsync_WithAttributes_CreatesRows()
    {
        var knowledgeNode = new KnowledgeNode
        {
            Id = Guid.NewGuid(),
            CanonicalName = "France",
            Attributes = new Dictionary<string, JsonValue?> { ["isoCode"] = JsonValue.Create("FR") }
        };

        await _repository.CreateAsync(knowledgeNode);

        var row = await _db.KnowledgeNodeAttribute.AsNoTracking().SingleAsync(a => a.KnowledgeNodeId == knowledgeNode.Id);
        Assert.That(row.Key, Is.EqualTo("isoCode"));
        Assert.That(row.Value!.GetValue<string>(), Is.EqualTo("FR"));
    }

    [Test]
    public async Task UpdateAsync_WhenNotFound_ReturnsNull()
    {
        var updated = await _repository.UpdateAsync(new KnowledgeNode { Id = Guid.NewGuid(), CanonicalName = "Mercury" });

        Assert.That(updated, Is.Null);
    }

    [Test]
    public async Task UpdateAsync_WithValidData_UpdatesAndReturnsKnowledgeNode()
    {
        var knowledgeNode = new KnowledgeNode { Id = Guid.NewGuid(), CanonicalName = "Mercury", Description = "Old" };
        await _db.KnowledgeNode.AddAsync(knowledgeNode);
        await _db.SaveChangesAsync();

        var updated = await _repository.UpdateAsync(new KnowledgeNode { Id = knowledgeNode.Id, CanonicalName = "Mercury", Description = "New description" });

        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Description, Is.EqualTo("New description"));
    }

    [Test]
    public async Task UpdateAsync_FullReplace_RemovesOmittedUpdatesChangedAndAddsNew()
    {
        var nodeTypeId = Guid.NewGuid();
        var knowledgeNode = new KnowledgeNode { Id = Guid.NewGuid(), NodeTypeId = nodeTypeId, CanonicalName = "France" };
        await _db.KnowledgeNode.AddAsync(knowledgeNode);

        await _db.KnowledgeNodeAttribute.AddRangeAsync(
            new KnowledgeNodeAttribute { KnowledgeNodeId = knowledgeNode.Id, Key = "isoCode", Value = JsonValue.Create("FR") },
            new KnowledgeNodeAttribute { KnowledgeNodeId = knowledgeNode.Id, Key = "population", Value = JsonValue.Create(1000) });
        await _db.SaveChangesAsync();

        var updated = await _repository.UpdateAsync(new KnowledgeNode
        {
            Id = knowledgeNode.Id,
            NodeTypeId = nodeTypeId,
            CanonicalName = "France",
            Attributes = new Dictionary<string, JsonValue?>
            {
                ["isoCode"] = JsonValue.Create("FR-NEW"),
                ["isEuMember"] = JsonValue.Create(true)
            }
        });

        Assert.That(updated, Is.Not.Null);
        var rows = await _db.KnowledgeNodeAttribute.AsNoTracking().Where(a => a.KnowledgeNodeId == knowledgeNode.Id).ToListAsync();
        Assert.That(rows.Select(r => r.Key), Is.EquivalentTo(new[] { "isoCode", "isEuMember" }));

        var isoRow = rows.Single(r => r.Key == "isoCode");
        Assert.That(isoRow.Value!.GetValue<string>(), Is.EqualTo("FR-NEW"));
    }

    [Test]
    public async Task DeleteAsync_WhenExists_RemovesKnowledgeNodeAndReturnsTrue()
    {
        var knowledgeNode = new KnowledgeNode { Id = Guid.NewGuid(), CanonicalName = "Mercury" };
        await _db.KnowledgeNode.AddAsync(knowledgeNode);
        await _db.SaveChangesAsync();

        var result = await _repository.DeleteAsync(knowledgeNode.Id);

        Assert.That(result, Is.True);
        Assert.That(await _db.KnowledgeNode.AsNoTracking().FirstOrDefaultAsync(n => n.Id == knowledgeNode.Id), Is.Null);
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
        _db.ThrowOnSaveChanges(PostgresExceptionFactory.UniqueViolation(constraintName: "uq_knowledge_node_node_type_id_canonical_name"));

        var ex = Assert.ThrowsAsync<ValidationException>(
            () => _repository.CreateAsync(new KnowledgeNode { Id = Guid.NewGuid(), CanonicalName = "Mercury" }));

        Assert.That(ex!.Message, Is.EqualTo("A KnowledgeNode with the same NodeType and CanonicalName already exists."));
    }

    [Test]
    public void CreateAsync_OnKnowledgeNodeAttributePrimaryKeyViolation_ThrowsValidationExceptionAboutDuplicateAttribute()
    {
        _db.ThrowOnSaveChanges(PostgresExceptionFactory.UniqueViolation(constraintName: "pk_knowledge_node_attribute"));

        var ex = Assert.ThrowsAsync<ValidationException>(() => _repository.CreateAsync(new KnowledgeNode
        {
            Id = Guid.NewGuid(),
            CanonicalName = "Mercury",
            Attributes = new Dictionary<string, JsonValue?> { ["isoCode"] = JsonValue.Create("FR") }
        }));

        Assert.That(ex!.Message, Is.EqualTo("An attribute with that key already exists for this KnowledgeNode."));
    }

    [Test]
    public void CreateAsync_OnForeignKeyViolationForKnowledgeNodeTable_ThrowsValidationExceptionAboutNodeType()
    {
        _db.ThrowOnSaveChanges(PostgresExceptionFactory.ForeignKeyViolation(tableName: "knowledge_node"));

        var ex = Assert.ThrowsAsync<ValidationException>(
            () => _repository.CreateAsync(new KnowledgeNode { Id = Guid.NewGuid(), CanonicalName = "Mercury" }));

        Assert.That(ex!.Message, Is.EqualTo("The specified NodeType does not exist."));
    }

    [Test]
    public async Task DeleteAsync_OnForeignKeyViolationForKnowledgeRelationTable_ThrowsValidationExceptionAboutReferences()
    {
        var knowledgeNode = new KnowledgeNode { Id = Guid.NewGuid(), CanonicalName = "Mercury" };
        await _db.KnowledgeNode.AddAsync(knowledgeNode);
        await _db.SaveChangesAsync();
        _db.ThrowOnExecuteDelete<KnowledgeNode>(PostgresExceptionFactory.ForeignKeyViolation(tableName: "knowledge_relation"));

        var ex = Assert.ThrowsAsync<ValidationException>(() => _repository.DeleteAsync(knowledgeNode.Id));

        Assert.That(ex!.Message, Is.EqualTo("The KnowledgeNode cannot be deleted because it is referenced by one or more KnowledgeRelations."));
    }
}
