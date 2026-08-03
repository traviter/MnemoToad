using Microsoft.AspNetCore.Mvc;
using MnemoToad.Knowledge.Api.Contracts;
using MnemoToad.Knowledge.Data.Entities;
using MnemoToad.Knowledge.Data.Repositories;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Knowledge.Api.Controllers;

[ApiController]
[Route("relationshipTypes")]
public class RelationshipTypesController : ControllerBase
{
    private readonly IRelationshipTypeRepository _repository;

    public RelationshipTypesController(IRelationshipTypeRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _repository.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) =>
        await _repository.GetByIdAsync(id) is { } relationshipType ? Ok(relationshipType) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(RelationshipTypeRequest request)
    {
        try
        {
            var created = await _repository.CreateAsync(new RelationshipType
            {
                Name = request.Name,
                InverseName = request.InverseName,
                Description = request.Description
            });
            return Created($"/relationshipTypes/{created.Id}", created);
        }
        catch (ValidationException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, RelationshipTypeRequest request)
    {
        try
        {
            var updated = await _repository.UpdateAsync(new RelationshipType
            {
                Id = id,
                Name = request.Name,
                InverseName = request.InverseName,
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
