using MnemoToad.Knowledge.Data.Entities;

namespace MnemoToad.Knowledge.Data.Repositories;

public class MediaAssetRepository : IMediaAssetRepository
{
    private readonly IAppDbContext _db;

    public MediaAssetRepository(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<MediaAsset?> GetByIdAsync(Guid id) =>
        await _db.MediaAsset.FindAsync(id);

    public async Task<MediaAsset> CreateAsync(MediaAsset mediaAsset)
    {
        _db.MediaAsset.Add(mediaAsset);
        await _db.SaveChangesAsync();
        return mediaAsset;
    }

    public async Task<MediaAsset?> UpdateAsync(MediaAsset mediaAsset)
    {
        var existing = await GetByIdAsync(mediaAsset.Id);
        if (existing is null) return null;

        existing.Url = mediaAsset.Url;
        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id) =>
        await _db.ExecuteDeleteAsync(_db.MediaAsset.Where(m => m.Id == id)) > 0;
}
