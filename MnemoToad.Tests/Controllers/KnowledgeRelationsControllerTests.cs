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
public class KnowledgeRelationsControllerTests
{
    private Mock<IKnowledgeRelationService> _service = null!;
    private KnowledgeRelationsController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new Mock<IKnowledgeRelationService>();
        _controller = new KnowledgeRelationsController(_service.Object);
    }

    [Test]
    public async Task GetByNodeId_ReturnsOkWithRelations()
    {
        var nodeId = Guid.NewGuid();
        var relations = new List<KnowledgeRelation> { new() { Id = Guid.NewGuid(), SourceNodeId = nodeId } };
        _service.Setup(s => s.GetByNodeIdAsync(nodeId)).ReturnsAsync(relations);

        var result = await _controller.GetByNodeId(nodeId);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.SameAs(relations));
    }

    [Test]
    public async Task Create_WithValidRequest_ReturnsCreatedWithRelation()
    {
        var sourceNodeId = Guid.NewGuid();
        var relationshipTypeId = Guid.NewGuid();
        var targetNodeId = Guid.NewGuid();
        var created = new KnowledgeRelation
        {
            Id = Guid.NewGuid(),
            SourceNodeId = sourceNodeId,
            RelationshipTypeId = relationshipTypeId,
            TargetNodeId = targetNodeId
        };
        _service.Setup(s => s.CreateAsync(It.Is<KnowledgeRelation>(r =>
                r.SourceNodeId == sourceNodeId && r.RelationshipTypeId == relationshipTypeId && r.TargetNodeId == targetNodeId)))
            .ReturnsAsync(created);

        var result = await _controller.Create(new KnowledgeRelationRequest(sourceNodeId, relationshipTypeId, targetNodeId));

        var createdResult = result as CreatedResult;
        Assert.That(createdResult, Is.Not.Null);
        Assert.That(createdResult!.Location, Is.EqualTo($"/relationships/{created.Id}"));
        Assert.That(createdResult.Value, Is.SameAs(created));
    }

    [Test]
    public async Task Create_WhenServiceThrowsValidationException_ReturnsBadRequest()
    {
        _service.Setup(s => s.CreateAsync(It.IsAny<KnowledgeRelation>()))
            .ThrowsAsync(new ValidationException("The specified source KnowledgeNode does not exist."));

        var result = await _controller.Create(new KnowledgeRelationRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

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
}
