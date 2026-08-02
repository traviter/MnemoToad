using Microsoft.AspNetCore.Mvc;
using MnemoToad.Api.Contracts;
using MnemoToad.Api.Services;
using MnemoToad.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Api.Controllers;

[ApiController]
[Route("nodes")]
public class KnowledgeNodesController : ControllerBase
{
    private readonly IKnowledgeNodeService _service;

    public KnowledgeNodesController(IKnowledgeNodeService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? nodeTypeId) =>
        Ok(await _service.GetAllAsync(nodeTypeId));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) =>
        await _service.GetByIdAsync(id) is { } knowledgeNode ? Ok(knowledgeNode) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(KnowledgeNodeRequest request)
    {
        try
        {
            var created = await _service.CreateAsync(new KnowledgeNode
            {
                NodeTypeId = request.NodeTypeId,
                CanonicalName = request.CanonicalName,
                Description = request.Description
            });
            return Created($"/nodes/{created.Id}", created);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, KnowledgeNodeRequest request)
    {
        try
        {
            var updated = await _service.UpdateAsync(new KnowledgeNode
            {
                Id = id,
                NodeTypeId = request.NodeTypeId,
                CanonicalName = request.CanonicalName,
                Description = request.Description
            });
            return updated is not null ? Ok(updated) : NotFound();
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        await _service.DeleteAsync(id) ? NoContent() : NotFound();
}
