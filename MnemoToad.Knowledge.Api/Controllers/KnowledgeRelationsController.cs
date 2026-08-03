using Microsoft.AspNetCore.Mvc;
using MnemoToad.Knowledge.Api.Contracts;
using MnemoToad.Knowledge.Data.Entities;
using MnemoToad.Knowledge.Data.Repositories;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Knowledge.Api.Controllers;

[ApiController]
[Route("relationships")]
public class KnowledgeRelationsController : ControllerBase
{
    private readonly IKnowledgeRelationRepository _repository;

    public KnowledgeRelationsController(IKnowledgeRelationRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("/nodes/{nodeId:guid}/relationships")]
    public async Task<IActionResult> GetByNodeId(Guid nodeId) =>
        Ok(await _repository.GetByNodeIdAsync(nodeId));

    [HttpPost]
    public async Task<IActionResult> Create(KnowledgeRelationRequest request)
    {
        try
        {
            var created = await _repository.CreateAsync(new KnowledgeRelation
            {
                SourceNodeId = request.SourceNodeId,
                RelationshipTypeId = request.RelationshipTypeId,
                TargetNodeId = request.TargetNodeId
            });
            return Created($"/relationships/{created.Id}", created);
        }
        catch (ValidationException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        await _repository.DeleteAsync(id) ? NoContent() : NotFound();
}
