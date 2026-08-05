using Microsoft.AspNetCore.Mvc;
using MnemoToad.Knowledge.Api.Contracts;
using MnemoToad.Knowledge.Data.Entities;
using MnemoToad.Knowledge.Tests.TestSupport;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;

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
    public async Task Create_ThenListForNode_RoundTripsThroughTheRealStack()
    {
        var nodeType = await _factory.Db.CreateNodeTypeAsync();
        var knowledgeNode = await _factory.Db.CreateKnowledgeNodeAsync(nodeType.Id);
        var attributeType = await _factory.Db.CreateAttributeTypeAsync();

        var createResponse = await _client.PostAsJsonAsync("/nodeAttributes",
            new KnowledgeNodeAttributeRequest(knowledgeNode.Id, attributeType.Id, "FR"));
        var created = await createResponse.Content.ReadFromJsonAsync<KnowledgeNodeAttribute>();

        Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var listResponse = await _client.GetAsync($"/nodes/{knowledgeNode.Id}/attributes");
        var list = await listResponse.Content.ReadFromJsonAsync<List<KnowledgeNodeAttribute>>();
        Assert.That(list!.Select(a => a.Id), Does.Contain(created!.Id));
    }

    [Test]
    public async Task Create_WithEmptyKnowledgeNodeId_Returns400WithValidationErrors()
    {
        var response = await _client.PostAsJsonAsync("/nodeAttributes",
            new KnowledgeNodeAttributeRequest(Guid.Empty, Guid.NewGuid(), "FR"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.That(problem!.Errors, Contains.Key("KnowledgeNodeId"));
    }

    [Test]
    public async Task Create_WhenRepositoryHitsKnowledgeNodeForeignKeyViolation_Returns400()
    {
        _factory.Db.ThrowOnSaveChanges(PostgresExceptionFactory.ForeignKeyViolation(constraintName: "fk_knowledge_node_attribute_knowledge_node_id"));

        var response = await _client.PostAsJsonAsync("/nodeAttributes",
            new KnowledgeNodeAttributeRequest(Guid.NewGuid(), Guid.NewGuid(), "FR"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.That(problem!.Detail, Is.EqualTo("The specified KnowledgeNode does not exist."));
    }

    [Test]
    public async Task Create_WhenRepositoryHitsAttributeTypeForeignKeyViolation_Returns400()
    {
        _factory.Db.ThrowOnSaveChanges(PostgresExceptionFactory.ForeignKeyViolation(constraintName: "fk_knowledge_node_attribute_attribute_type_id"));

        var response = await _client.PostAsJsonAsync("/nodeAttributes",
            new KnowledgeNodeAttributeRequest(Guid.NewGuid(), Guid.NewGuid(), "FR"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.That(problem!.Detail, Is.EqualTo("The specified AttributeType does not exist."));
    }

    [Test]
    public async Task Create_WhenRepositoryHitsUniqueViolation_Returns400()
    {
        _factory.Db.ThrowOnSaveChanges(PostgresExceptionFactory.UniqueViolation());

        var response = await _client.PostAsJsonAsync("/nodeAttributes",
            new KnowledgeNodeAttributeRequest(Guid.NewGuid(), Guid.NewGuid(), "FR"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.That(problem!.Detail, Is.EqualTo("An attribute of that type already exists for this KnowledgeNode."));
    }

    [Test]
    public async Task Update_WhenNotFound_Returns404()
    {
        var response = await _client.PutAsJsonAsync($"/nodeAttributes/{Guid.NewGuid()}",
            new KnowledgeNodeAttributeRequest(Guid.NewGuid(), Guid.NewGuid(), "FR"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Update_WhenExists_UpdatesValue()
    {
        var nodeType = await _factory.Db.CreateNodeTypeAsync();
        var knowledgeNode = await _factory.Db.CreateKnowledgeNodeAsync(nodeType.Id);
        var attributeType = await _factory.Db.CreateAttributeTypeAsync();
        var knowledgeNodeAttribute = await _factory.Db.CreateKnowledgeNodeAttributeAsync(knowledgeNode.Id, attributeType.Id, "FR");

        var response = await _client.PutAsJsonAsync($"/nodeAttributes/{knowledgeNodeAttribute.Id}",
            new KnowledgeNodeAttributeRequest(knowledgeNode.Id, attributeType.Id, "68000000"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updated = await response.Content.ReadFromJsonAsync<KnowledgeNodeAttribute>();
        Assert.That(updated!.Value, Is.EqualTo("68000000"));
    }

    [Test]
    public async Task Delete_WhenExists_Returns204AndRemovesItFromNodeListing()
    {
        var nodeType = await _factory.Db.CreateNodeTypeAsync();
        var knowledgeNode = await _factory.Db.CreateKnowledgeNodeAsync(nodeType.Id);
        var attributeType = await _factory.Db.CreateAttributeTypeAsync();
        var knowledgeNodeAttribute = await _factory.Db.CreateKnowledgeNodeAttributeAsync(knowledgeNode.Id, attributeType.Id);

        var deleteResponse = await _client.DeleteAsync($"/nodeAttributes/{knowledgeNodeAttribute.Id}");

        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        var listResponse = await _client.GetAsync($"/nodes/{knowledgeNode.Id}/attributes");
        var list = await listResponse.Content.ReadFromJsonAsync<List<KnowledgeNodeAttribute>>();
        Assert.That(list!.Select(a => a.Id), Does.Not.Contain(knowledgeNodeAttribute.Id));
    }

    [Test]
    public async Task Delete_WhenNotFound_Returns404()
    {
        var response = await _client.DeleteAsync($"/nodeAttributes/{Guid.NewGuid()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
