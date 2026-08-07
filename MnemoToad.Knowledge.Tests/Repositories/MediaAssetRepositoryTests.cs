using Microsoft.EntityFrameworkCore;
using MnemoToad.Knowledge.Data.Entities;
using MnemoToad.Knowledge.Data.Repositories;
using MnemoToad.Knowledge.Tests.TestSupport;
using NUnit.Framework;

namespace MnemoToad.Knowledge.Tests.Repositories;

[TestFixture]
public class MediaAssetRepositoryTests
{
    private MockableAppDbContext _db = null!;
    private MediaAssetRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        _db = new MockableAppDbContext();
        _repository = new MediaAssetRepository(_db);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    [Test]
    public async Task GetByIdAsync_WhenExists_ReturnsMediaAsset()
    {
        var mediaAsset = new MediaAsset { Id = Guid.NewGuid(), Url = "https://example.com/fr.svg" };
        await _db.MediaAsset.AddAsync(mediaAsset);
        await _db.SaveChangesAsync();

        var found = await _repository.GetByIdAsync(mediaAsset.Id);

        Assert.That(found?.Url, Is.EqualTo("https://example.com/fr.svg"));
    }

    [Test]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        var found = await _repository.GetByIdAsync(Guid.NewGuid());

        Assert.That(found, Is.Null);
    }

    [Test]
    public async Task CreateAsync_PersistsAndReturnsMediaAsset()
    {
        var mediaAsset = new MediaAsset { Id = Guid.NewGuid(), Url = "https://example.com/fr.svg" };

        var created = await _repository.CreateAsync(mediaAsset);

        Assert.That(created, Is.SameAs(mediaAsset));
        Assert.That(await _db.MediaAsset.FindAsync(mediaAsset.Id), Is.Not.Null);
    }

    [Test]
    public async Task UpdateAsync_WhenNotFound_ReturnsNull()
    {
        var updated = await _repository.UpdateAsync(new MediaAsset { Id = Guid.NewGuid(), Url = "https://example.com/fr.svg" });

        Assert.That(updated, Is.Null);
    }

    [Test]
    public async Task UpdateAsync_WithValidData_UpdatesAndReturnsMediaAsset()
    {
        var mediaAsset = new MediaAsset { Id = Guid.NewGuid(), Url = "https://example.com/fr.svg" };
        await _db.MediaAsset.AddAsync(mediaAsset);
        await _db.SaveChangesAsync();

        var updated = await _repository.UpdateAsync(new MediaAsset { Id = mediaAsset.Id, Url = "https://example.com/fr-new.svg" });

        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Url, Is.EqualTo("https://example.com/fr-new.svg"));
    }

    [Test]
    public async Task DeleteAsync_WhenExists_RemovesMediaAssetAndReturnsTrue()
    {
        var mediaAsset = new MediaAsset { Id = Guid.NewGuid(), Url = "https://example.com/fr.svg" };
        await _db.MediaAsset.AddAsync(mediaAsset);
        await _db.SaveChangesAsync();

        var result = await _repository.DeleteAsync(mediaAsset.Id);

        Assert.That(result, Is.True);
        Assert.That(await _db.MediaAsset.AsNoTracking().FirstOrDefaultAsync(m => m.Id == mediaAsset.Id), Is.Null);
    }

    [Test]
    public async Task DeleteAsync_WhenNotFound_ReturnsFalse()
    {
        var result = await _repository.DeleteAsync(Guid.NewGuid());

        Assert.That(result, Is.False);
    }
}
