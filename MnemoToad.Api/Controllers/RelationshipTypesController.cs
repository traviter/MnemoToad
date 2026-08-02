using Microsoft.AspNetCore.Mvc;
using MnemoToad.Api.Contracts;
using MnemoToad.Api.Services;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Api.Controllers;

[ApiController]
[Route("relationshipTypes")]
public class RelationshipTypesController : ControllerBase
{
    private readonly IRelationshipTypeService _service;

    public RelationshipTypesController(IRelationshipTypeService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) =>
        await _service.GetByIdAsync(id) is { } relationshipType ? Ok(relationshipType) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(RelationshipTypeRequest request)
    {
        try
        {
            var created = await _service.CreateAsync(request.Name, request.InverseName, request.Description);
            return Created($"/relationshipTypes/{created.Id}", created);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, RelationshipTypeRequest request)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, request.Name, request.InverseName, request.Description);
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
