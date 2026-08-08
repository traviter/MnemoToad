using Microsoft.AspNetCore.Mvc;
using MnemoToad.Knowledge.Api.Contracts;
using MnemoToad.Knowledge.Data.Entities;
using MnemoToad.Knowledge.Tests.TestSupport;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace MnemoToad.Knowledge.Tests.SystemTests;

[TestFixture]
public class KnowledgeRelationsControllerSystemTests
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
    public async Task Create_ThenListForSourceAndTargetNode_RoundTripsThroughTheRealStack()
    {
        var nodeType = await _factory.Db.CreateNodeTypeAsync();
        var sourceNode = await _factory.Db.CreateKnowledgeNodeAsync(nodeType.Id);
        var targetNode = await _factory.Db.CreateKnowledgeNodeAsync(nodeType.Id);
        var relationshipType = await _factory.Db.CreateRelationshipTypeAsync();

        var createResponse = await _client.PostAsJsonAsync("/relationships",
            new KnowledgeRelationRequest(sourceNode.Id, relationshipType.Id, targetNode.Id));
        var created = await createResponse.Content.ReadFromJsonAsync<KnowledgeRelation>();

        Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var sourceListResponse = await _client.GetAsync($"/nodes/{sourceNode.Id}/relationships");
        var sourceList = await sourceListResponse.Content.ReadFromJsonAsync<List<KnowledgeRelation>>();
        Assert.That(sourceList!.Select(r => r.Id), Does.Contain(created!.Id));

        var targetListResponse = await _client.GetAsync($"/nodes/{targetNode.Id}/relationships");
        var targetList = await targetListResponse.Content.ReadFromJsonAsync<List<KnowledgeRelation>>();
        Assert.That(targetList!.Select(r => r.Id), Does.Contain(created.Id));
    }

    [Test]
    public async Task Create_WithInvalidSourceNodeId_Returns400WithValidationErrors()
    {
        var json = "{\"sourceNodeId\":\"not-a-guid\",\"relationshipTypeId\":\"" + Guid.NewGuid() + "\",\"targetNodeId\":\"" + Guid.NewGuid() + "\"}";

        var response = await _client.PostAsync("/relationships", new StringContent(json, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.That(problem!.Errors, Contains.Key("$.sourceNodeId"));
    }

    [Test]
    public async Task Create_WithMissingSourceNodeId_Returns400WithValidationErrors()
    {
        var json = "{\"relationshipTypeId\":\"" + Guid.NewGuid() + "\",\"targetNodeId\":\"" + Guid.NewGuid() + "\"}";

        var response = await _client.PostAsync("/relationships", new StringContent(json, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.That(problem!.Errors, Contains.Key("SourceNodeId"));
    }

    [Test]
    public async Task Create_WithInvalidRelationshipTypeId_Returns400WithValidationErrors()
    {
        var json = "{\"sourceNodeId\":\"" + Guid.NewGuid() + "\",\"relationshipTypeId\":\"not-a-guid\",\"targetNodeId\":\"" + Guid.NewGuid() + "\"}";

        var response = await _client.PostAsync("/relationships", new StringContent(json, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.That(problem!.Errors, Contains.Key("$.relationshipTypeId"));
    }

    [Test]
    public async Task Create_WithMissingRelationshipTypeId_Returns400WithValidationErrors()
    {
        var json = "{\"sourceNodeId\":\"" + Guid.NewGuid() + "\",\"targetNodeId\":\"" + Guid.NewGuid() + "\"}";

        var response = await _client.PostAsync("/relationships", new StringContent(json, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.That(problem!.Errors, Contains.Key("RelationshipTypeId"));
    }

    [Test]
    public async Task Create_WithInvalidTargetNodeId_Returns400WithValidationErrors()
    {
        var json = "{\"sourceNodeId\":\"" + Guid.NewGuid() + "\",\"relationshipTypeId\":\"" + Guid.NewGuid() + "\",\"targetNodeId\":\"not-a-guid\"}";

        var response = await _client.PostAsync("/relationships", new StringContent(json, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.That(problem!.Errors, Contains.Key("$.targetNodeId"));
    }

    [Test]
    public async Task Create_WithMissingTargetNodeId_Returns400WithValidationErrors()
    {
        var json = "{\"sourceNodeId\":\"" + Guid.NewGuid() + "\",\"relationshipTypeId\":\"" + Guid.NewGuid() + "\"}";

        var response = await _client.PostAsync("/relationships", new StringContent(json, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.That(problem!.Errors, Contains.Key("TargetNodeId"));
    }

    [Test]
    public async Task Create_WhenRepositoryHitsSourceNodeForeignKeyViolation_Returns400()
    {
        _factory.Db.ThrowOnSaveChanges(PostgresExceptionFactory.ForeignKeyViolation(constraintName: "fk_knowledge_relation_source_node_id"));

        var response = await _client.PostAsJsonAsync("/relationships",
            new KnowledgeRelationRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.That(problem!.Detail, Is.EqualTo("The specified source KnowledgeNode does not exist."));
    }

    [Test]
    public async Task Create_WhenRepositoryHitsTargetNodeForeignKeyViolation_Returns400()
    {
        _factory.Db.ThrowOnSaveChanges(PostgresExceptionFactory.ForeignKeyViolation(constraintName: "fk_knowledge_relation_target_node_id"));

        var response = await _client.PostAsJsonAsync("/relationships",
            new KnowledgeRelationRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.That(problem!.Detail, Is.EqualTo("The specified target KnowledgeNode does not exist."));
    }

    [Test]
    public async Task Create_WhenRepositoryHitsRelationshipTypeForeignKeyViolation_Returns400()
    {
        _factory.Db.ThrowOnSaveChanges(PostgresExceptionFactory.ForeignKeyViolation(constraintName: "fk_knowledge_relation_relationship_type_id"));

        var response = await _client.PostAsJsonAsync("/relationships",
            new KnowledgeRelationRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.That(problem!.Detail, Is.EqualTo("The specified RelationshipType does not exist."));
    }

    [Test]
    public async Task Create_WhenRepositoryHitsUniqueViolation_Returns400()
    {
        _factory.Db.ThrowOnSaveChanges(PostgresExceptionFactory.UniqueViolation());

        var response = await _client.PostAsJsonAsync("/relationships",
            new KnowledgeRelationRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.That(problem!.Detail, Is.EqualTo("A KnowledgeRelation with the same SourceNode, RelationshipType, and TargetNode already exists."));
    }

    [Test]
    public async Task Delete_WhenExists_Returns204AndRemovesItFromNodeListing()
    {
        var nodeType = await _factory.Db.CreateNodeTypeAsync();
        var sourceNode = await _factory.Db.CreateKnowledgeNodeAsync(nodeType.Id);
        var targetNode = await _factory.Db.CreateKnowledgeNodeAsync(nodeType.Id);
        var relationshipType = await _factory.Db.CreateRelationshipTypeAsync();
        var knowledgeRelation = await _factory.Db.CreateKnowledgeRelationAsync(sourceNode.Id, relationshipType.Id, targetNode.Id);

        var deleteResponse = await _client.DeleteAsync($"/relationships/{knowledgeRelation.Id}");

        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        var listResponse = await _client.GetAsync($"/nodes/{sourceNode.Id}/relationships");
        var list = await listResponse.Content.ReadFromJsonAsync<List<KnowledgeRelation>>();
        Assert.That(list!.Select(r => r.Id), Does.Not.Contain(knowledgeRelation.Id));
    }

    [Test]
    public async Task Delete_WhenNotFound_Returns404()
    {
        var response = await _client.DeleteAsync($"/relationships/{Guid.NewGuid()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
