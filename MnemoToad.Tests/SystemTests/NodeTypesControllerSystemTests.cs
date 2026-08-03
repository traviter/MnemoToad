using Microsoft.AspNetCore.Mvc;
using MnemoToad.Api.Contracts;
using MnemoToad.Data.Entities;
using MnemoToad.Tests.TestSupport;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;

namespace MnemoToad.Tests.SystemTests;

[TestFixture]
public class NodeTypesControllerSystemTests
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
        var response = await _client.GetAsync($"/nodeTypes/{Guid.NewGuid()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Create_ThenGetById_RoundTripsThroughTheRealStack()
    {
        var createResponse = await _client.PostAsJsonAsync("/nodeTypes", new NodeTypeRequest("Person", "A human being"));
        var created = await createResponse.Content.ReadFromJsonAsync<NodeType>();

        var getResponse = await _client.GetAsync($"/nodeTypes/{created!.Id}");

        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var fetched = await getResponse.Content.ReadFromJsonAsync<NodeType>();
        Assert.That(fetched!.Name, Is.EqualTo("Person"));
    }

    [Test]
    public async Task Create_WithBlankName_Returns400WithValidationErrors()
    {
        var response = await _client.PostAsJsonAsync("/nodeTypes", new NodeTypeRequest("", null));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.That(problem!.Errors, Contains.Key("Name"));
    }

    [Test]
    public async Task Create_WhenRepositoryHitsUniqueViolation_Returns400()
    {
        _factory.Db.ThrowOnSaveChanges(PostgresExceptionFactory.UniqueViolation());

        var response = await _client.PostAsJsonAsync("/nodeTypes", new NodeTypeRequest("Person", null));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.That(problem!.Detail, Is.EqualTo("A NodeType with that name already exists."));
    }

    [Test]
    public async Task Update_WhenNotFound_Returns404()
    {
        var response = await _client.PutAsJsonAsync($"/nodeTypes/{Guid.NewGuid()}", new NodeTypeRequest("Person", null));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_WhenNotFound_Returns404()
    {
        var response = await _client.DeleteAsync($"/nodeTypes/{Guid.NewGuid()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_WhenExists_Returns204AndRemovesIt()
    {
        var createResponse = await _client.PostAsJsonAsync("/nodeTypes", new NodeTypeRequest("Person", null));
        var created = await createResponse.Content.ReadFromJsonAsync<NodeType>();

        var deleteResponse = await _client.DeleteAsync($"/nodeTypes/{created!.Id}");

        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(await _factory.Db.NodeType.FindAsync(created.Id), Is.Null);
    }

    [Test]
    public async Task Delete_WhenRepositoryHitsForeignKeyViolation_Returns400()
    {
        var createResponse = await _client.PostAsJsonAsync("/nodeTypes", new NodeTypeRequest("Person", null));
        var created = await createResponse.Content.ReadFromJsonAsync<NodeType>();
        _factory.Db.ThrowOnSaveChanges(PostgresExceptionFactory.ForeignKeyViolation());

        var response = await _client.DeleteAsync($"/nodeTypes/{created!.Id}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}
