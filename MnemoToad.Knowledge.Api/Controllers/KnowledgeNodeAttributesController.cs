using Microsoft.AspNetCore.Mvc;
using MnemoToad.Knowledge.Api.Contracts;
using MnemoToad.Knowledge.Data.Entities;
using MnemoToad.Knowledge.Data.Repositories;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Knowledge.Api.Controllers;

[ApiController]
[Route("nodeAttributes")]
public class KnowledgeNodeAttributesController : ControllerBase
{
    private readonly IKnowledgeNodeAttributeRepository _repository;

    public KnowledgeNodeAttributesController(IKnowledgeNodeAttributeRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("/nodes/{nodeId:guid}/attributes")]
    public async Task<IActionResult> GetByNodeId(Guid nodeId) =>
        Ok(await _repository.GetByNodeIdAsync(nodeId));

    [HttpPost]
    public async Task<IActionResult> Create(KnowledgeNodeAttributeRequest request)
    {
        try
        {
            var created = await _repository.CreateAsync(new KnowledgeNodeAttribute
            {
                KnowledgeNodeId = request.KnowledgeNodeId,
                AttributeTypeId = request.AttributeTypeId,
                Value = request.Value
            });
            return Created($"/nodeAttributes/{created.Id}", created);
        }
        catch (ValidationException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, KnowledgeNodeAttributeRequest request)
    {
        try
        {
            var updated = await _repository.UpdateAsync(new KnowledgeNodeAttribute
            {
                Id = id,
                KnowledgeNodeId = request.KnowledgeNodeId,
                AttributeTypeId = request.AttributeTypeId,
                Value = request.Value
            });
            return updated is not null ? Ok(updated) : NotFound();
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
