using Microsoft.AspNetCore.Mvc;
using Moq;
using MnemoToad.Knowledge.Api.Contracts;
using MnemoToad.Knowledge.Api.Controllers;
using MnemoToad.Knowledge.Data.Entities;
using MnemoToad.Knowledge.Data.Repositories;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Knowledge.Tests.Controllers;

[TestFixture]
public class KnowledgeNodesControllerTests
{
    private Mock<IKnowledgeNodeRepository> _repository = null!;
    private KnowledgeNodesController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new Mock<IKnowledgeNodeRepository>();
        _controller = new KnowledgeNodesController(_repository.Object);
    }

    [Test]
    public async Task GetAll_WithNoFilter_ReturnsOkWithAllNodes()
    {
        var nodes = new List<KnowledgeNode> { new() { Id = Guid.NewGuid(), CanonicalName = "Mercury" } };
        _repository.Setup(r => r.GetAllAsync(null)).ReturnsAsync(nodes);

        var result = await _controller.GetAll(null);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.SameAs(nodes));
    }

    [Test]
    public async Task GetAll_WithNodeTypeIdFilter_PassesFilterToRepository()
    {
        var nodeTypeId = Guid.NewGuid();
        var nodes = new List<KnowledgeNode>();
        _repository.Setup(r => r.GetAllAsync(nodeTypeId)).ReturnsAsync(nodes);

        var result = await _controller.GetAll(nodeTypeId);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.SameAs(nodes));
        _repository.Verify(r => r.GetAllAsync(nodeTypeId), Times.Once);
    }

    [Test]
    public async Task GetById_WhenExists_ReturnsOkWithNode()
    {
        var node = new KnowledgeNode { Id = Guid.NewGuid(), CanonicalName = "Mercury" };
        _repository.Setup(r => r.GetByIdAsync(node.Id)).ReturnsAsync(node);

        var result = await _controller.GetById(node.Id);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.SameAs(node));
    }

    [Test]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((KnowledgeNode?)null);

        var result = await _controller.GetById(Guid.NewGuid());

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Create_WithValidRequest_ReturnsCreatedWithNode()
    {
        var nodeTypeId = Guid.NewGuid();
        var created = new KnowledgeNode { Id = Guid.NewGuid(), NodeTypeId = nodeTypeId, CanonicalName = "Mercury" };
        _repository.Setup(r => r.CreateAsync(It.Is<KnowledgeNode>(n => n.NodeTypeId == nodeTypeId && n.CanonicalName == "Mercury")))
            .ReturnsAsync(created);

        var result = await _controller.Create(new KnowledgeNodeRequest(nodeTypeId, "Mercury", null));

        var createdResult = result as CreatedResult;
        Assert.That(createdResult, Is.Not.Null);
        Assert.That(createdResult!.Location, Is.EqualTo($"/nodes/{created.Id}"));
        Assert.That(createdResult.Value, Is.SameAs(created));
    }

    [Test]
    public async Task Create_WhenRepositoryThrowsValidationException_ReturnsProblem400()
    {
        _repository.Setup(r => r.CreateAsync(It.IsAny<KnowledgeNode>()))
            .ThrowsAsync(new ValidationException("A KnowledgeNode with the same NodeType and CanonicalName already exists."));

        var result = await _controller.Create(new KnowledgeNodeRequest(Guid.NewGuid(), "Mercury", null));

        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult!.StatusCode, Is.EqualTo(400));
        var problem = objectResult.Value as ProblemDetails;
        Assert.That(problem, Is.Not.Null);
        Assert.That(problem!.Detail, Is.EqualTo("A KnowledgeNode with the same NodeType and CanonicalName already exists."));
    }

    [Test]
    public async Task Update_WhenExists_ReturnsOkWithUpdatedNode()
    {
        var id = Guid.NewGuid();
        var updated = new KnowledgeNode { Id = id, CanonicalName = "Mercury", Description = "New description" };
        _repository.Setup(r => r.UpdateAsync(It.Is<KnowledgeNode>(n => n.Id == id))).ReturnsAsync(updated);

        var result = await _controller.Update(id, new KnowledgeNodeRequest(Guid.NewGuid(), "Mercury", "New description"));

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.SameAs(updated));
    }

    [Test]
    public async Task Update_WhenNotFound_ReturnsNotFound()
    {
        _repository.Setup(r => r.UpdateAsync(It.IsAny<KnowledgeNode>())).ReturnsAsync((KnowledgeNode?)null);

        var result = await _controller.Update(Guid.NewGuid(), new KnowledgeNodeRequest(Guid.NewGuid(), "Mercury", null));

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Update_WhenRepositoryThrowsValidationException_ReturnsProblem400()
    {
        _repository.Setup(r => r.UpdateAsync(It.IsAny<KnowledgeNode>()))
            .ThrowsAsync(new ValidationException("The specified NodeType does not exist."));

        var result = await _controller.Update(Guid.NewGuid(), new KnowledgeNodeRequest(Guid.NewGuid(), "Mercury", null));

        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult!.StatusCode, Is.EqualTo(400));
        var problem = objectResult.Value as ProblemDetails;
        Assert.That(problem, Is.Not.Null);
        Assert.That(problem!.Detail, Is.EqualTo("The specified NodeType does not exist."));
    }

    [Test]
    public async Task Delete_WhenExists_ReturnsNoContent()
    {
        _repository.Setup(r => r.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(true);

        var result = await _controller.Delete(Guid.NewGuid());

        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task Delete_WhenNotFound_ReturnsNotFound()
    {
        _repository.Setup(r => r.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(false);

        var result = await _controller.Delete(Guid.NewGuid());

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Delete_WhenRepositoryThrowsValidationException_ReturnsProblem400()
    {
        _repository.Setup(r => r.DeleteAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new ValidationException("The KnowledgeNode cannot be deleted because it is referenced by one or more KnowledgeRelations."));

        var result = await _controller.Delete(Guid.NewGuid());

        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult!.StatusCode, Is.EqualTo(400));
        var problem = objectResult.Value as ProblemDetails;
        Assert.That(problem, Is.Not.Null);
        Assert.That(problem!.Detail, Is.EqualTo("The KnowledgeNode cannot be deleted because it is referenced by one or more KnowledgeRelations."));
    }
}
