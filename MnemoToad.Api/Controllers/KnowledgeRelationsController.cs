using Microsoft.AspNetCore.Mvc;
using MnemoToad.Api.Contracts;
using MnemoToad.Api.Services;
using MnemoToad.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Api.Controllers;

[ApiController]
[Route("relationships")]
public class KnowledgeRelationsController : ControllerBase
{
    private readonly IKnowledgeRelationService _service;

    public KnowledgeRelationsController(IKnowledgeRelationService service)
    {
        _service = service;
    }

    [HttpGet("/nodes/{nodeId:guid}/relationships")]
    public async Task<IActionResult> GetByNodeId(Guid nodeId) =>
        Ok(await _service.GetByNodeIdAsync(nodeId));

    [HttpPost]
    public async Task<IActionResult> Create(KnowledgeRelationRequest request)
    {
        try
        {
            var created = await _service.CreateAsync(new KnowledgeRelation
            {
                SourceNodeId = request.SourceNodeId,
                RelationshipTypeId = request.RelationshipTypeId,
                TargetNodeId = request.TargetNodeId
            });
            return Created($"/relationships/{created.Id}", created);
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
