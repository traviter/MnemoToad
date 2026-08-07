using Microsoft.AspNetCore.Mvc;
using MnemoToad.Knowledge.Api.Contracts;
using MnemoToad.Knowledge.Data.Entities;
using MnemoToad.Knowledge.Data.Repositories;

namespace MnemoToad.Knowledge.Api.Controllers;

[ApiController]
[Route("mediaAssets")]
public class MediaAssetsController : ControllerBase
{
    private readonly IMediaAssetRepository _repository;

    public MediaAssetsController(IMediaAssetRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) =>
        await _repository.GetByIdAsync(id) is { } mediaAsset ? Ok(mediaAsset) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(MediaAssetRequest request)
    {
        var created = await _repository.CreateAsync(new MediaAsset { Url = request.Url });
        return Created($"/mediaAssets/{created.Id}", created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, MediaAssetRequest request)
    {
        var updated = await _repository.UpdateAsync(new MediaAsset { Id = id, Url = request.Url });
        return updated is not null ? Ok(updated) : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        await _repository.DeleteAsync(id) ? NoContent() : NotFound();
}
