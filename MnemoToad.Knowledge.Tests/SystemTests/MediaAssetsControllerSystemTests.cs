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
public class MediaAssetsControllerSystemTests
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
        var response = await _client.GetAsync($"/mediaAssets/{Guid.NewGuid()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Create_ThenGetById_RoundTripsThroughTheRealStack()
    {
        var createResponse = await _client.PostAsJsonAsync("/mediaAssets", new MediaAssetRequest("https://example.com/fr.svg"));
        var created = await createResponse.Content.ReadFromJsonAsync<MediaAsset>();

        var getResponse = await _client.GetAsync($"/mediaAssets/{created!.Id}");

        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var fetched = await getResponse.Content.ReadFromJsonAsync<MediaAsset>();
        Assert.That(fetched!.Url, Is.EqualTo("https://example.com/fr.svg"));
    }

    [Test]
    public async Task Create_WithBlankUrl_Returns400WithValidationErrors()
    {
        var response = await _client.PostAsJsonAsync("/mediaAssets", new MediaAssetRequest(""));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.That(problem!.Errors, Contains.Key("Url"));
    }

    [Test]
    public async Task Update_WhenNotFound_Returns404()
    {
        var response = await _client.PutAsJsonAsync($"/mediaAssets/{Guid.NewGuid()}", new MediaAssetRequest("https://example.com/fr.svg"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Update_WhenExists_UpdatesAndReturnsIt()
    {
        var mediaAsset = await _factory.Db.CreateMediaAssetAsync();

        var response = await _client.PutAsJsonAsync($"/mediaAssets/{mediaAsset.Id}", new MediaAssetRequest("https://example.com/fr-new.svg"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updated = await response.Content.ReadFromJsonAsync<MediaAsset>();
        Assert.That(updated!.Url, Is.EqualTo("https://example.com/fr-new.svg"));
    }

    [Test]
    public async Task Delete_WhenExists_Returns204AndRemovesIt()
    {
        var mediaAsset = await _factory.Db.CreateMediaAssetAsync();

        var deleteResponse = await _client.DeleteAsync($"/mediaAssets/{mediaAsset.Id}");

        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(await _factory.Db.MediaAsset.AsNoTracking().FirstOrDefaultAsync(m => m.Id == mediaAsset.Id), Is.Null);
    }

    [Test]
    public async Task Delete_WhenNotFound_Returns404()
    {
        var response = await _client.DeleteAsync($"/mediaAssets/{Guid.NewGuid()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
