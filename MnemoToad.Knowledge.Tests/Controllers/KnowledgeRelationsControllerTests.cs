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
public class KnowledgeRelationsControllerTests
{
    private Mock<IKnowledgeRelationRepository> _repository = null!;
    private KnowledgeRelationsController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new Mock<IKnowledgeRelationRepository>();
        _controller = new KnowledgeRelationsController(_repository.Object);
    }

    [Test]
    public async Task GetByNodeId_ReturnsOkWithRelations()
    {
        var nodeId = Guid.NewGuid();
        var relations = new List<KnowledgeRelation> { new() { Id = Guid.NewGuid(), SourceNodeId = nodeId } };
        _repository.Setup(r => r.GetByNodeIdAsync(nodeId)).ReturnsAsync(relations);

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
        _repository.Setup(r => r.CreateAsync(It.Is<KnowledgeRelation>(k =>
                k.SourceNodeId == sourceNodeId && k.RelationshipTypeId == relationshipTypeId && k.TargetNodeId == targetNodeId)))
            .ReturnsAsync(created);

        var result = await _controller.Create(new KnowledgeRelationRequest(sourceNodeId, relationshipTypeId, targetNodeId));

        var createdResult = result as CreatedResult;
        Assert.That(createdResult, Is.Not.Null);
        Assert.That(createdResult!.Location, Is.EqualTo($"/relationships/{created.Id}"));
        Assert.That(createdResult.Value, Is.SameAs(created));
    }

    [Test]
    public async Task Create_WhenRepositoryThrowsValidationException_ReturnsProblem400()
    {
        _repository.Setup(r => r.CreateAsync(It.IsAny<KnowledgeRelation>()))
            .ThrowsAsync(new ValidationException("The specified source KnowledgeNode does not exist."));

        var result = await _controller.Create(new KnowledgeRelationRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult!.StatusCode, Is.EqualTo(400));
        var problem = objectResult.Value as ProblemDetails;
        Assert.That(problem, Is.Not.Null);
        Assert.That(problem!.Detail, Is.EqualTo("The specified source KnowledgeNode does not exist."));
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
