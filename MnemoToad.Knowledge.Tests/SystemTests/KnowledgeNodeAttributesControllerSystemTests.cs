using MnemoToad.Knowledge.Tests.TestSupport;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace MnemoToad.Knowledge.Tests.SystemTests;

[TestFixture]
public class KnowledgeNodeAttributesControllerSystemTests
{
    private MockedDbWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new MockedDbWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task GetByNodeId_ReturnsAttributesKeyedByTypeName()
    {
        var nodeType = await _factory.Db.CreateNodeTypeAsync();
        var knowledgeNode = await _factory.Db.CreateKnowledgeNodeAsync(nodeType.Id);
        await _factory.Db.CreateKnowledgeNodeAttributeAsync(knowledgeNode.Id, "isoCode", JsonValue.Create("FR"));

        var response = await _client.GetAsync($"/nodes/{knowledgeNode.Id}/attributes");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var attributes = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonValue?>>();
        Assert.That(attributes!["isoCode"]!.GetValue<string>(), Is.EqualTo("FR"));
    }

    [Test]
    public async Task GetByNodeId_WhenNoAttributes_ReturnsEmptyObject()
    {
        var nodeType = await _factory.Db.CreateNodeTypeAsync();
        var knowledgeNode = await _factory.Db.CreateKnowledgeNodeAsync(nodeType.Id);

        var response = await _client.GetAsync($"/nodes/{knowledgeNode.Id}/attributes");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var attributes = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonValue?>>();
        Assert.That(attributes, Is.Empty);
    }
}
