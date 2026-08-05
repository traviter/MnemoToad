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
public class KnowledgeNodeAttributesControllerTests
{
    private Mock<IKnowledgeNodeAttributeRepository> _repository = null!;
    private KnowledgeNodeAttributesController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new Mock<IKnowledgeNodeAttributeRepository>();
        _controller = new KnowledgeNodeAttributesController(_repository.Object);
    }

    [Test]
    public async Task GetByNodeId_ReturnsOkWithAttributes()
    {
        var nodeId = Guid.NewGuid();
        var attributes = new List<KnowledgeNodeAttribute> { new() { Id = Guid.NewGuid(), KnowledgeNodeId = nodeId, Value = "FR" } };
        _repository.Setup(r => r.GetByNodeIdAsync(nodeId)).ReturnsAsync(attributes);

        var result = await _controller.GetByNodeId(nodeId);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.SameAs(attributes));
    }

    [Test]
    public async Task Create_WithValidRequest_ReturnsCreatedWithAttribute()
    {
        var knowledgeNodeId = Guid.NewGuid();
        var attributeTypeId = Guid.NewGuid();
        var created = new KnowledgeNodeAttribute { Id = Guid.NewGuid(), KnowledgeNodeId = knowledgeNodeId, AttributeTypeId = attributeTypeId, Value = "FR" };
        _repository.Setup(r => r.CreateAsync(It.Is<KnowledgeNodeAttribute>(a =>
                a.KnowledgeNodeId == knowledgeNodeId && a.AttributeTypeId == attributeTypeId && a.Value == "FR")))
            .ReturnsAsync(created);

        var result = await _controller.Create(new KnowledgeNodeAttributeRequest(knowledgeNodeId, attributeTypeId, "FR"));

        var createdResult = result as CreatedResult;
        Assert.That(createdResult, Is.Not.Null);
        Assert.That(createdResult!.Location, Is.EqualTo($"/nodeAttributes/{created.Id}"));
        Assert.That(createdResult.Value, Is.SameAs(created));
    }

    [Test]
    public async Task Create_WhenRepositoryThrowsValidationException_ReturnsProblem400()
    {
        _repository.Setup(r => r.CreateAsync(It.IsAny<KnowledgeNodeAttribute>()))
            .ThrowsAsync(new ValidationException("The specified KnowledgeNode does not exist."));

        var result = await _controller.Create(new KnowledgeNodeAttributeRequest(Guid.NewGuid(), Guid.NewGuid(), "FR"));

        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult!.StatusCode, Is.EqualTo(400));
        var problem = objectResult.Value as ProblemDetails;
        Assert.That(problem, Is.Not.Null);
        Assert.That(problem!.Detail, Is.EqualTo("The specified KnowledgeNode does not exist."));
    }

    [Test]
    public async Task Update_WhenExists_ReturnsOkWithUpdatedAttribute()
    {
        var id = Guid.NewGuid();
        var updated = new KnowledgeNodeAttribute { Id = id, Value = "68000000" };
        _repository.Setup(r => r.UpdateAsync(It.Is<KnowledgeNodeAttribute>(a => a.Id == id))).ReturnsAsync(updated);

        var result = await _controller.Update(id, new KnowledgeNodeAttributeRequest(Guid.NewGuid(), Guid.NewGuid(), "68000000"));

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.SameAs(updated));
    }

    [Test]
    public async Task Update_WhenNotFound_ReturnsNotFound()
    {
        _repository.Setup(r => r.UpdateAsync(It.IsAny<KnowledgeNodeAttribute>())).ReturnsAsync((KnowledgeNodeAttribute?)null);

        var result = await _controller.Update(Guid.NewGuid(), new KnowledgeNodeAttributeRequest(Guid.NewGuid(), Guid.NewGuid(), "FR"));

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Update_WhenRepositoryThrowsValidationException_ReturnsProblem400()
    {
        _repository.Setup(r => r.UpdateAsync(It.IsAny<KnowledgeNodeAttribute>()))
            .ThrowsAsync(new ValidationException("The specified AttributeType does not exist."));

        var result = await _controller.Update(Guid.NewGuid(), new KnowledgeNodeAttributeRequest(Guid.NewGuid(), Guid.NewGuid(), "FR"));

        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult!.StatusCode, Is.EqualTo(400));
        var problem = objectResult.Value as ProblemDetails;
        Assert.That(problem, Is.Not.Null);
        Assert.That(problem!.Detail, Is.EqualTo("The specified AttributeType does not exist."));
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
}
