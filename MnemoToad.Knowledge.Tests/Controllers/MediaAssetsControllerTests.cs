using Microsoft.AspNetCore.Mvc;
using Moq;
using MnemoToad.Knowledge.Api.Contracts;
using MnemoToad.Knowledge.Api.Controllers;
using MnemoToad.Knowledge.Data.Entities;
using MnemoToad.Knowledge.Data.Repositories;
using NUnit.Framework;

namespace MnemoToad.Knowledge.Tests.Controllers;

[TestFixture]
public class MediaAssetsControllerTests
{
    private Mock<IMediaAssetRepository> _repository = null!;
    private MediaAssetsController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new Mock<IMediaAssetRepository>();
        _controller = new MediaAssetsController(_repository.Object);
    }

    [Test]
    public async Task GetById_WhenExists_ReturnsOkWithMediaAsset()
    {
        var mediaAsset = new MediaAsset { Id = Guid.NewGuid(), Url = "https://example.com/fr.svg" };
        _repository.Setup(r => r.GetByIdAsync(mediaAsset.Id)).ReturnsAsync(mediaAsset);

        var result = await _controller.GetById(mediaAsset.Id);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.SameAs(mediaAsset));
    }

    [Test]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((MediaAsset?)null);

        var result = await _controller.GetById(Guid.NewGuid());

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Create_WithValidRequest_ReturnsCreatedWithMediaAsset()
    {
        var created = new MediaAsset { Id = Guid.NewGuid(), Url = "https://example.com/fr.svg" };
        _repository.Setup(r => r.CreateAsync(It.Is<MediaAsset>(m => m.Url == "https://example.com/fr.svg")))
            .ReturnsAsync(created);

        var result = await _controller.Create(new MediaAssetRequest("https://example.com/fr.svg"));

        var createdResult = result as CreatedResult;
        Assert.That(createdResult, Is.Not.Null);
        Assert.That(createdResult!.Location, Is.EqualTo($"/mediaAssets/{created.Id}"));
        Assert.That(createdResult.Value, Is.SameAs(created));
    }

    [Test]
    public async Task Update_WhenExists_ReturnsOkWithUpdatedMediaAsset()
    {
        var id = Guid.NewGuid();
        var updated = new MediaAsset { Id = id, Url = "https://example.com/fr-new.svg" };
        _repository.Setup(r => r.UpdateAsync(It.Is<MediaAsset>(m => m.Id == id))).ReturnsAsync(updated);

        var result = await _controller.Update(id, new MediaAssetRequest("https://example.com/fr-new.svg"));

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.SameAs(updated));
    }

    [Test]
    public async Task Update_WhenNotFound_ReturnsNotFound()
    {
        _repository.Setup(r => r.UpdateAsync(It.IsAny<MediaAsset>())).ReturnsAsync((MediaAsset?)null);

        var result = await _controller.Update(Guid.NewGuid(), new MediaAssetRequest("https://example.com/fr.svg"));

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Delete_WhenExists_ReturnsNoContent()
    {
        _repository.Setup(r => r.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(true);

        var result = await _controller.Delete(Guid.NewGuid());

        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task Delete_WhenNotFound_ReturnsNotFound()
    {
        _repository.Setup(r => r.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(false);

        var result = await _controller.Delete(Guid.NewGuid());

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }
}
