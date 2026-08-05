using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MnemoToad.Knowledge.Api.Contracts;
using MnemoToad.Knowledge.Data.Entities;
using MnemoToad.Knowledge.Tests.TestSupport;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;

namespace MnemoToad.Knowledge.Tests.SystemTests;

[TestFixture]
public class AttributeTypesControllerSystemTests
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
        var response = await _client.GetAsync($"/attributeTypes/{Guid.NewGuid()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Create_ThenGetById_RoundTripsThroughTheRealStack()
    {
        var createResponse = await _client.PostAsJsonAsync("/attributeTypes", new AttributeTypeRequest("Population", "A country's population"));
        var created = await createResponse.Content.ReadFromJsonAsync<AttributeType>();

        var getResponse = await _client.GetAsync($"/attributeTypes/{created!.Id}");

        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var fetched = await getResponse.Content.ReadFromJsonAsync<AttributeType>();
        Assert.That(fetched!.Name, Is.EqualTo("Population"));
    }

    [Test]
    public async Task Create_WithBlankName_Returns400WithValidationErrors()
    {
        var response = await _client.PostAsJsonAsync("/attributeTypes", new AttributeTypeRequest("", null));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.That(problem!.Errors, Contains.Key("Name"));
    }

    [Test]
    public async Task Create_WhenRepositoryHitsUniqueViolation_Returns400()
    {
        _factory.Db.ThrowOnSaveChanges(PostgresExceptionFactory.UniqueViolation());

        var response = await _client.PostAsJsonAsync("/attributeTypes", new AttributeTypeRequest("Population", null));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.That(problem!.Detail, Is.EqualTo("An AttributeType with that name already exists."));
    }

    [Test]
    public async Task Update_WhenNotFound_Returns404()
    {
        var response = await _client.PutAsJsonAsync($"/attributeTypes/{Guid.NewGuid()}", new AttributeTypeRequest("Population", null));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_WhenNotFound_Returns404()
    {
        var response = await _client.DeleteAsync($"/attributeTypes/{Guid.NewGuid()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_WhenExists_Returns204AndRemovesIt()
    {
        var attributeType = await _factory.Db.CreateAttributeTypeAsync();

        var deleteResponse = await _client.DeleteAsync($"/attributeTypes/{attributeType.Id}");

        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(await _factory.Db.AttributeType.AsNoTracking().FirstOrDefaultAsync(a => a.Id == attributeType.Id), Is.Null);
    }

    [Test]
    public async Task Delete_WhenRepositoryHitsForeignKeyViolation_Returns400()
    {
        var attributeType = await _factory.Db.CreateAttributeTypeAsync();
        _factory.Db.ThrowOnExecuteDelete<AttributeType>(PostgresExceptionFactory.ForeignKeyViolation());

        var response = await _client.DeleteAsync($"/attributeTypes/{attributeType.Id}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}
