using Microsoft.AspNetCore.Mvc;
using MnemoToad.Api.Contracts;
using MnemoToad.Data.Entities;
using MnemoToad.Data.Repositories;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Api.Controllers;

[ApiController]
[Route("nodes")]
public class KnowledgeNodesController : ControllerBase
{
    private readonly IKnowledgeNodeRepository _repository;

    public KnowledgeNodesController(IKnowledgeNodeRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? nodeTypeId) =>
        Ok(await _repository.GetAllAsync(nodeTypeId));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) =>
        await _repository.GetByIdAsync(id) is { } knowledgeNode ? Ok(knowledgeNode) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(KnowledgeNodeRequest request)
    {
        try
        {
            var created = await _repository.CreateAsync(new KnowledgeNode
            {
                NodeTypeId = request.NodeTypeId,
                CanonicalName = request.CanonicalName,
                Description = request.Description
            });
            return Created($"/nodes/{created.Id}", created);
        }
        catch (ValidationException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, KnowledgeNodeRequest request)
    {
        try
        {
            var updated = await _repository.UpdateAsync(new KnowledgeNode
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
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            return await _repository.DeleteAsync(id) ? NoContent() : NotFound();
        }
        catch (ValidationException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }
}
