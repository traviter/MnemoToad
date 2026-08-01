using Microsoft.AspNetCore.Mvc;
using MnemoToad.Api.Contracts;
using MnemoToad.Api.Services;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Api.Controllers
{
    [ApiController]
    [Route("nodeTypes")]
    public class NodeTypesController : ControllerBase
    {
        private readonly NodeTypeService _service;

        public NodeTypesController(NodeTypeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _service.GetAllAsync());

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id) =>
            await _service.GetByIdAsync(id) is { } nodeType ? Ok(nodeType) : NotFound();

        [HttpPost]
        public async Task<IActionResult> Create(NodeTypeRequest request)
        {
            try
            {
                var created = await _service.CreateAsync(request.Name, request.Description);
                return Created($"/nodeTypes/{created.Id}", created);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, NodeTypeRequest request)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, request.Name, request.Description);
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
}
