using Microsoft.AspNetCore.Mvc;
using MnemoToad.Knowledge.Api.Contracts;
using MnemoToad.Knowledge.Data.Entities;
using MnemoToad.Knowledge.Data.Repositories;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Knowledge.Api.Controllers;

[ApiController]
[Route("attributeTypes")]
public class AttributeTypesController : ControllerBase
{
    private readonly IAttributeTypeRepository _repository;

    public AttributeTypesController(IAttributeTypeRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _repository.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) =>
        await _repository.GetByIdAsync(id) is { } attributeType ? Ok(attributeType) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(AttributeTypeRequest request)
    {
        try
        {
            var created = await _repository.CreateAsync(new AttributeType { Name = request.Name, Description = request.Description });
            return Created($"/attributeTypes/{created.Id}", created);
        }
        catch (ValidationException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, AttributeTypeRequest request)
    {
        try
        {
            var updated = await _repository.UpdateAsync(new AttributeType { Id = id, Name = request.Name, Description = request.Description });
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
