using Microsoft.AspNetCore.Mvc;
using Moq;
using MnemoToad.Api.Contracts;
using MnemoToad.Api.Controllers;
using MnemoToad.Api.Services;
using MnemoToad.Data.Entities;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Tests.Controllers;

[TestFixture]
public class NodeTypesControllerTests
{
    private Mock<INodeTypeService> _service = null!;
    private NodeTypesController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new Mock<INodeTypeService>();
        _controller = new NodeTypesController(_service.Object);
    }

    [Test]
    public async Task GetAll_ReturnsOkWithNodeTypes()
    {
        var nodeTypes = new List<NodeType> { new() { Id = Guid.NewGuid(), Name = "Person" } };
        _service.Setup(s => s.GetAllAsync()).ReturnsAsync(nodeTypes);

        var result = await _controller.GetAll();

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.SameAs(nodeTypes));
    }

    [Test]
    public async Task GetById_WhenExists_ReturnsOkWithNodeType()
    {
        var nodeType = new NodeType { Id = Guid.NewGuid(), Name = "Person" };
        _service.Setup(s => s.GetByIdAsync(nodeType.Id)).ReturnsAsync(nodeType);

        var result = await _controller.GetById(nodeType.Id);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.SameAs(nodeType));
    }

    [Test]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        _service.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((NodeType?)null);

        var result = await _controller.GetById(Guid.NewGuid());

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Create_WithValidRequest_ReturnsCreatedWithNodeType()
    {
        var created = new NodeType { Id = Guid.NewGuid(), Name = "Person", Description = "A human being" };
        _service.Setup(s => s.CreateAsync(It.Is<NodeType>(n => n.Name == "Person" && n.Description == "A human being")))
            .ReturnsAsync(created);

        var result = await _controller.Create(new NodeTypeRequest("Person", "A human being"));

        var createdResult = result as CreatedResult;
        Assert.That(createdResult, Is.Not.Null);
        Assert.That(createdResult!.Location, Is.EqualTo($"/nodeTypes/{created.Id}"));
        Assert.That(createdResult.Value, Is.SameAs(created));
    }

    [Test]
    public async Task Create_WhenServiceThrowsValidationException_ReturnsBadRequest()
    {
        _service.Setup(s => s.CreateAsync(It.IsAny<NodeType>()))
            .ThrowsAsync(new ValidationException("A NodeType with that name already exists."));

        var result = await _controller.Create(new NodeTypeRequest("Person", null));

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Update_WhenExists_ReturnsOkWithUpdatedNodeType()
    {
        var id = Guid.NewGuid();
        var updated = new NodeType { Id = id, Name = "Person", Description = "New description" };
        _service.Setup(s => s.UpdateAsync(It.Is<NodeType>(n => n.Id == id))).ReturnsAsync(updated);

        var result = await _controller.Update(id, new NodeTypeRequest("Person", "New description"));

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.SameAs(updated));
    }

    [Test]
    public async Task Update_WhenNotFound_ReturnsNotFound()
    {
        _service.Setup(s => s.UpdateAsync(It.IsAny<NodeType>())).ReturnsAsync((NodeType?)null);

        var result = await _controller.Update(Guid.NewGuid(), new NodeTypeRequest("Person", null));

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Update_WhenServiceThrowsValidationException_ReturnsBadRequest()
    {
        _service.Setup(s => s.UpdateAsync(It.IsAny<NodeType>()))
            .ThrowsAsync(new ValidationException("A NodeType with that name already exists."));

        var result = await _controller.Update(Guid.NewGuid(), new NodeTypeRequest("Person", null));

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Delete_WhenExists_ReturnsNoContent()
    {
        _service.Setup(s => s.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(true);

        var result = await _controller.Delete(Guid.NewGuid());

        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task Delete_WhenNotFound_ReturnsNotFound()
    {
        _service.Setup(s => s.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(false);

        var result = await _controller.Delete(Guid.NewGuid());

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Delete_WhenServiceThrowsValidationException_ReturnsBadRequest()
    {
        _service.Setup(s => s.DeleteAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new ValidationException("The NodeType cannot be deleted because it is referenced by one or more KnowledgeNodes."));

        var result = await _controller.Delete(Guid.NewGuid());

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }
}
