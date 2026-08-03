using Microsoft.AspNetCore.Mvc;
using MnemoToad.Api.Contracts;
using MnemoToad.Data.Entities;
using MnemoToad.Tests.TestSupport;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;

namespace MnemoToad.Tests.SystemTests;

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

    private async Task<Guid> CreateNodeTypeAsync()
    {
        var response = await _client.PostAsJsonAsync("/nodeTypes", new NodeTypeRequest($"NodeType_{Guid.NewGuid()}", null));
        var created = await response.Content.ReadFromJsonAsync<NodeType>();
        return created!.Id;
    }

    private async Task<Guid> CreateKnowledgeNodeAsync(Guid nodeTypeId)
    {
        var response = await _client.PostAsJsonAsync("/nodes", new KnowledgeNodeRequest(nodeTypeId, $"Node_{Guid.NewGuid()}", null));
        var created = await response.Content.ReadFromJsonAsync<KnowledgeNode>();
        return created!.Id;
    }

    private async Task<Guid> CreateRelationshipTypeAsync()
    {
        var response = await _client.PostAsJsonAsync("/relationshipTypes", new RelationshipTypeRequest($"RelationshipType_{Guid.NewGuid()}", null, null));
        var created = await response.Content.ReadFromJsonAsync<RelationshipType>();
        return created!.Id;
    }

    [Test]
    public async Task Create_ThenListForSourceAndTargetNode_RoundTripsThroughTheRealStack()
    {
        var nodeTypeId = await CreateNodeTypeAsync();
        var sourceNodeId = await CreateKnowledgeNodeAsync(nodeTypeId);
        var targetNodeId = await CreateKnowledgeNodeAsync(nodeTypeId);
        var relationshipTypeId = await CreateRelationshipTypeAsync();

        var createResponse = await _client.PostAsJsonAsync("/relationships",
            new KnowledgeRelationRequest(sourceNodeId, relationshipTypeId, targetNodeId));
        var created = await createResponse.Content.ReadFromJsonAsync<KnowledgeRelation>();

        Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var sourceListResponse = await _client.GetAsync($"/nodes/{sourceNodeId}/relationships");
        var sourceList = await sourceListResponse.Content.ReadFromJsonAsync<List<KnowledgeRelation>>();
        Assert.That(sourceList!.Select(r => r.Id), Does.Contain(created!.Id));

        var targetListResponse = await _client.GetAsync($"/nodes/{targetNodeId}/relationships");
        var targetList = await targetListResponse.Content.ReadFromJsonAsync<List<KnowledgeRelation>>();
        Assert.That(targetList!.Select(r => r.Id), Does.Contain(created.Id));
    }

    [Test]
    public async Task Create_WithEmptySourceNodeId_Returns400WithValidationErrors()
    {
        var response = await _client.PostAsJsonAsync("/relationships",
            new KnowledgeRelationRequest(Guid.Empty, Guid.NewGuid(), Guid.NewGuid()));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.That(problem!.Errors, Contains.Key("SourceNodeId"));
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
        var nodeTypeId = await CreateNodeTypeAsync();
        var sourceNodeId = await CreateKnowledgeNodeAsync(nodeTypeId);
        var targetNodeId = await CreateKnowledgeNodeAsync(nodeTypeId);
        var relationshipTypeId = await CreateRelationshipTypeAsync();
        var createResponse = await _client.PostAsJsonAsync("/relationships",
            new KnowledgeRelationRequest(sourceNodeId, relationshipTypeId, targetNodeId));
        var created = await createResponse.Content.ReadFromJsonAsync<KnowledgeRelation>();

        var deleteResponse = await _client.DeleteAsync($"/relationships/{created!.Id}");

        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        var listResponse = await _client.GetAsync($"/nodes/{sourceNodeId}/relationships");
        var list = await listResponse.Content.ReadFromJsonAsync<List<KnowledgeRelation>>();
        Assert.That(list!.Select(r => r.Id), Does.Not.Contain(created.Id));
    }

    [Test]
    public async Task Delete_WhenNotFound_Returns404()
    {
        var response = await _client.DeleteAsync($"/relationships/{Guid.NewGuid()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
