using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MnemoToad.Api.Contracts;
using MnemoToad.Data.Entities;
using MnemoToad.Tests.TestSupport;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;

namespace MnemoToad.Tests.SystemTests;

[TestFixture]
public class KnowledgeNodesControllerSystemTests
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
    public async Task GetById_WhenNotFound_Returns404()
    {
        var response = await _client.GetAsync($"/nodes/{Guid.NewGuid()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Create_ThenGetById_RoundTripsThroughTheRealStack()
    {
        var nodeType = await _factory.Db.CreateNodeTypeAsync();

        var createResponse = await _client.PostAsJsonAsync("/nodes", new KnowledgeNodeRequest(nodeType.Id, "Mercury", "The planet"));
        var created = await createResponse.Content.ReadFromJsonAsync<KnowledgeNode>();

        var getResponse = await _client.GetAsync($"/nodes/{created!.Id}");

        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var fetched = await getResponse.Content.ReadFromJsonAsync<KnowledgeNode>();
        Assert.That(fetched!.CanonicalName, Is.EqualTo("Mercury"));
    }

    [Test]
    public async Task Create_WithBlankCanonicalName_Returns400WithValidationErrors()
    {
        var nodeType = await _factory.Db.CreateNodeTypeAsync();

        var response = await _client.PostAsJsonAsync("/nodes", new KnowledgeNodeRequest(nodeType.Id, "", null));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.That(problem!.Errors, Contains.Key("CanonicalName"));
    }

    [Test]
    public async Task Create_WithEmptyNodeTypeId_Returns400WithValidationErrors()
    {
        var response = await _client.PostAsJsonAsync("/nodes", new KnowledgeNodeRequest(Guid.Empty, "Mercury", null));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.That(problem!.Errors, Contains.Key("NodeTypeId"));
    }

    [Test]
    public async Task Create_WhenRepositoryHitsUniqueViolation_Returns400()
    {
        var nodeType = await _factory.Db.CreateNodeTypeAsync();
        _factory.Db.ThrowOnSaveChanges(PostgresExceptionFactory.UniqueViolation());

        var response = await _client.PostAsJsonAsync("/nodes", new KnowledgeNodeRequest(nodeType.Id, "Mercury", null));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.That(problem!.Detail, Is.EqualTo("A KnowledgeNode with the same NodeType and CanonicalName already exists."));
    }

    [Test]
    public async Task Create_WhenRepositoryHitsNodeTypeForeignKeyViolation_Returns400()
    {
        _factory.Db.ThrowOnSaveChanges(PostgresExceptionFactory.ForeignKeyViolation(tableName: "knowledge_node"));

        var response = await _client.PostAsJsonAsync("/nodes", new KnowledgeNodeRequest(Guid.NewGuid(), "Mercury", null));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.That(problem!.Detail, Is.EqualTo("The specified NodeType does not exist."));
    }

    [Test]
    public async Task GetAll_WithNodeTypeIdQueryParam_ReturnsOnlyMatchingNodes()
    {
        var nodeType1 = await _factory.Db.CreateNodeTypeAsync();
        var nodeType2 = await _factory.Db.CreateNodeTypeAsync();
        await _factory.Db.CreateKnowledgeNodeAsync(nodeType1.Id, "Mercury");
        await _factory.Db.CreateKnowledgeNodeAsync(nodeType2.Id, "Venus");

        var response = await _client.GetAsync($"/nodes?nodeTypeId={nodeType1.Id}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var nodes = await response.Content.ReadFromJsonAsync<List<KnowledgeNode>>();
        Assert.That(nodes!.Select(n => n.CanonicalName), Is.EqualTo(new[] { "Mercury" }));
    }

    [Test]
    public async Task Update_WhenNotFound_Returns404()
    {
        var response = await _client.PutAsJsonAsync($"/nodes/{Guid.NewGuid()}", new KnowledgeNodeRequest(Guid.NewGuid(), "Mercury", null));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Update_WhenRepositoryHitsUniqueViolation_Returns400()
    {
        var nodeType = await _factory.Db.CreateNodeTypeAsync();
        var knowledgeNode = await _factory.Db.CreateKnowledgeNodeAsync(nodeType.Id, "Mercury");
        _factory.Db.ThrowOnSaveChanges(PostgresExceptionFactory.UniqueViolation());

        var response = await _client.PutAsJsonAsync($"/nodes/{knowledgeNode.Id}", new KnowledgeNodeRequest(nodeType.Id, "Venus", null));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Update_WhenRepositoryHitsNodeTypeForeignKeyViolation_Returns400()
    {
        var nodeType = await _factory.Db.CreateNodeTypeAsync();
        var knowledgeNode = await _factory.Db.CreateKnowledgeNodeAsync(nodeType.Id, "Mercury");
        _factory.Db.ThrowOnSaveChanges(PostgresExceptionFactory.ForeignKeyViolation(tableName: "knowledge_node"));

        var response = await _client.PutAsJsonAsync($"/nodes/{knowledgeNode.Id}", new KnowledgeNodeRequest(Guid.NewGuid(), "Mercury", null));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Delete_WhenExists_Returns204AndRemovesIt()
    {
        var nodeType = await _factory.Db.CreateNodeTypeAsync();
        var knowledgeNode = await _factory.Db.CreateKnowledgeNodeAsync(nodeType.Id);

        var deleteResponse = await _client.DeleteAsync($"/nodes/{knowledgeNode.Id}");

        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(await _factory.Db.KnowledgeNode.AsNoTracking().FirstOrDefaultAsync(n => n.Id == knowledgeNode.Id), Is.Null);
    }

    [Test]
    public async Task Delete_WhenRepositoryHitsKnowledgeRelationForeignKeyViolation_Returns400()
    {
        var nodeType = await _factory.Db.CreateNodeTypeAsync();
        var knowledgeNode = await _factory.Db.CreateKnowledgeNodeAsync(nodeType.Id);
        _factory.Db.ThrowOnExecuteDelete<KnowledgeNode>(PostgresExceptionFactory.ForeignKeyViolation(tableName: "knowledge_relation"));

        var response = await _client.DeleteAsync($"/nodes/{knowledgeNode.Id}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}
