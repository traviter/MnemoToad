using Microsoft.AspNetCore.Mvc;
using MnemoToad.Knowledge.Api.Contracts;
using MnemoToad.Knowledge.Data.Entities;
using MnemoToad.Knowledge.Data.Repositories;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Knowledge.Api.Controllers;

[ApiController]
[Route("nodeTypes")]
public class NodeTypesController : ControllerBase
{
    private readonly INodeTypeRepository _repository;

    public NodeTypesController(INodeTypeRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _repository.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) =>
        await _repository.GetByIdAsync(id) is { } nodeType ? Ok(nodeType) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(NodeTypeRequest request)
    {
        try
        {
            var created = await _repository.CreateAsync(new NodeType { Name = request.Name, Description = request.Description });
            return Created($"/nodeTypes/{created.Id}", created);
        }
        catch (ValidationException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, NodeTypeRequest request)
    {
        try
        {
            var updated = await _repository.UpdateAsync(new NodeType { Id = id, Name = request.Name, Description = request.Description });
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
