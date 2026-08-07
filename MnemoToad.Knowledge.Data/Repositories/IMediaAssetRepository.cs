using MnemoToad.Knowledge.Data.Entities;

namespace MnemoToad.Knowledge.Data.Repositories;

public interface IMediaAssetRepository
{
    Task<MediaAsset?> GetByIdAsync(Guid id);
    Task<MediaAsset> CreateAsync(MediaAsset mediaAsset);
    Task<MediaAsset?> UpdateAsync(MediaAsset mediaAsset);
    Task<bool> DeleteAsync(Guid id);
}
