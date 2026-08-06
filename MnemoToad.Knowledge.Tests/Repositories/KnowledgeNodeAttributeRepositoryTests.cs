using MnemoToad.Knowledge.Data.Entities;
using MnemoToad.Knowledge.Data.Repositories;
using MnemoToad.Knowledge.Tests.TestSupport;
using NUnit.Framework;
using System.Text.Json.Nodes;

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
    public async Task GetByNodeIdAsync_ReturnsOnlyAttributesForThatNodeKeyedByKey()
    {
        var knowledgeNodeId = Guid.NewGuid();
        await _db.KnowledgeNodeAttribute.AddRangeAsync(
            new KnowledgeNodeAttribute { KnowledgeNodeId = knowledgeNodeId, Key = "isoCode", Value = JsonValue.Create("FR") },
            new KnowledgeNodeAttribute { KnowledgeNodeId = Guid.NewGuid(), Key = "otherAttribute", Value = JsonValue.Create("DE") });
        await _db.SaveChangesAsync();

        var found = await _repository.GetByNodeIdAsync(knowledgeNodeId);

        Assert.That(found.Keys, Is.EquivalentTo(new[] { "isoCode" }));
        Assert.That(found["isoCode"]!.GetValue<string>(), Is.EqualTo("FR"));
    }

    [Test]
    public async Task GetByNodeIdAsync_WhenNoAttributes_ReturnsEmptyDictionary()
    {
        var found = await _repository.GetByNodeIdAsync(Guid.NewGuid());

        Assert.That(found, Is.Empty);
    }
}
