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
public class KnowledgeNodesControllerTests
{
    private Mock<IKnowledgeNodeService> _service = null!;
    private KnowledgeNodesController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new Mock<IKnowledgeNodeService>();
        _controller = new KnowledgeNodesController(_service.Object);
    }

    [Test]
    public async Task GetAll_WithNoFilter_ReturnsOkWithAllNodes()
    {
        var nodes = new List<KnowledgeNode> { new() { Id = Guid.NewGuid(), CanonicalName = "Mercury" } };
        _service.Setup(s => s.GetAllAsync(null)).ReturnsAsync(nodes);

        var result = await _controller.GetAll(null);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.SameAs(nodes));
    }

    [Test]
    public async Task GetAll_WithNodeTypeIdFilter_PassesFilterToService()
    {
        var nodeTypeId = Guid.NewGuid();
        var nodes = new List<KnowledgeNode>();
        _service.Setup(s => s.GetAllAsync(nodeTypeId)).ReturnsAsync(nodes);

        var result = await _controller.GetAll(nodeTypeId);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.SameAs(nodes));
        _service.Verify(s => s.GetAllAsync(nodeTypeId), Times.Once);
    }

    [Test]
    public async Task GetById_WhenExists_ReturnsOkWithNode()
    {
        var node = new KnowledgeNode { Id = Guid.NewGuid(), CanonicalName = "Mercury" };
        _service.Setup(s => s.GetByIdAsync(node.Id)).ReturnsAsync(node);

        var result = await _controller.GetById(node.Id);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.SameAs(node));
    }

    [Test]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        _service.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((KnowledgeNode?)null);

        var result = await _controller.GetById(Guid.NewGuid());

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Create_WithValidRequest_ReturnsCreatedWithNode()
    {
        var nodeTypeId = Guid.NewGuid();
        var created = new KnowledgeNode { Id = Guid.NewGuid(), NodeTypeId = nodeTypeId, CanonicalName = "Mercury" };
        _service.Setup(s => s.CreateAsync(It.Is<KnowledgeNode>(n => n.NodeTypeId == nodeTypeId && n.CanonicalName == "Mercury")))
            .ReturnsAsync(created);

        var result = await _controller.Create(new KnowledgeNodeRequest(nodeTypeId, "Mercury", null));

        var createdResult = result as CreatedResult;
        Assert.That(createdResult, Is.Not.Null);
        Assert.That(createdResult!.Location, Is.EqualTo($"/nodes/{created.Id}"));
        Assert.That(createdResult.Value, Is.SameAs(created));
    }

    [Test]
    public async Task Create_WhenServiceThrowsValidationException_ReturnsBadRequest()
    {
        _service.Setup(s => s.CreateAsync(It.IsAny<KnowledgeNode>()))
            .ThrowsAsync(new ValidationException("A KnowledgeNode with the same NodeType and CanonicalName already exists."));

        var result = await _controller.Create(new KnowledgeNodeRequest(Guid.NewGuid(), "Mercury", null));

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Update_WhenExists_ReturnsOkWithUpdatedNode()
    {
        var id = Guid.NewGuid();
        var updated = new KnowledgeNode { Id = id, CanonicalName = "Mercury", Description = "New description" };
        _service.Setup(s => s.UpdateAsync(It.Is<KnowledgeNode>(n => n.Id == id))).ReturnsAsync(updated);

        var result = await _controller.Update(id, new KnowledgeNodeRequest(Guid.NewGuid(), "Mercury", "New description"));

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.SameAs(updated));
    }

    [Test]
    public async Task Update_WhenNotFound_ReturnsNotFound()
    {
        _service.Setup(s => s.UpdateAsync(It.IsAny<KnowledgeNode>())).ReturnsAsync((KnowledgeNode?)null);

        var result = await _controller.Update(Guid.NewGuid(), new KnowledgeNodeRequest(Guid.NewGuid(), "Mercury", null));

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Update_WhenServiceThrowsValidationException_ReturnsBadRequest()
    {
        _service.Setup(s => s.UpdateAsync(It.IsAny<KnowledgeNode>()))
            .ThrowsAsync(new ValidationException("The specified NodeType does not exist."));

        var result = await _controller.Update(Guid.NewGuid(), new KnowledgeNodeRequest(Guid.NewGuid(), "Mercury", null));

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
            .ThrowsAsync(new ValidationException("The KnowledgeNode cannot be deleted because it is referenced by one or more KnowledgeRelations."));

        var result = await _controller.Delete(Guid.NewGuid());

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }
}
