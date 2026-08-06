using Microsoft.AspNetCore.Mvc;
using MnemoToad.Knowledge.Data.Repositories;

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
}
