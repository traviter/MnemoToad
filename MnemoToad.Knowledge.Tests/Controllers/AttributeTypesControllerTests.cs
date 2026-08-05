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
public class AttributeTypesControllerTests
{
    private Mock<IAttributeTypeRepository> _repository = null!;
    private AttributeTypesController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new Mock<IAttributeTypeRepository>();
        _controller = new AttributeTypesController(_repository.Object);
    }

    [Test]
    public async Task GetAll_ReturnsOkWithAttributeTypes()
    {
        var attributeTypes = new List<AttributeType> { new() { Id = Guid.NewGuid(), Name = "Population" } };
        _repository.Setup(r => r.GetAllAsync()).ReturnsAsync(attributeTypes);

        var result = await _controller.GetAll();

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.SameAs(attributeTypes));
    }

    [Test]
    public async Task GetById_WhenExists_ReturnsOkWithAttributeType()
    {
        var attributeType = new AttributeType { Id = Guid.NewGuid(), Name = "Population" };
        _repository.Setup(r => r.GetByIdAsync(attributeType.Id)).ReturnsAsync(attributeType);

        var result = await _controller.GetById(attributeType.Id);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.SameAs(attributeType));
    }

    [Test]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((AttributeType?)null);

        var result = await _controller.GetById(Guid.NewGuid());

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Create_WithValidRequest_ReturnsCreatedWithAttributeType()
    {
        var created = new AttributeType { Id = Guid.NewGuid(), Name = "Population", Description = "A country's population" };
        _repository.Setup(r => r.CreateAsync(It.Is<AttributeType>(a => a.Name == "Population" && a.Description == "A country's population")))
            .ReturnsAsync(created);

        var result = await _controller.Create(new AttributeTypeRequest("Population", "A country's population"));

        var createdResult = result as CreatedResult;
        Assert.That(createdResult, Is.Not.Null);
        Assert.That(createdResult!.Location, Is.EqualTo($"/attributeTypes/{created.Id}"));
        Assert.That(createdResult.Value, Is.SameAs(created));
    }

    [Test]
    public async Task Create_WhenRepositoryThrowsValidationException_ReturnsProblem400()
    {
        _repository.Setup(r => r.CreateAsync(It.IsAny<AttributeType>()))
            .ThrowsAsync(new ValidationException("An AttributeType with that name already exists."));

        var result = await _controller.Create(new AttributeTypeRequest("Population", null));

        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult!.StatusCode, Is.EqualTo(400));
        var problem = objectResult.Value as ProblemDetails;
        Assert.That(problem, Is.Not.Null);
        Assert.That(problem!.Detail, Is.EqualTo("An AttributeType with that name already exists."));
    }

    [Test]
    public async Task Update_WhenExists_ReturnsOkWithUpdatedAttributeType()
    {
        var id = Guid.NewGuid();
        var updated = new AttributeType { Id = id, Name = "Population", Description = "New description" };
        _repository.Setup(r => r.UpdateAsync(It.Is<AttributeType>(a => a.Id == id))).ReturnsAsync(updated);

        var result = await _controller.Update(id, new AttributeTypeRequest("Population", "New description"));

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.SameAs(updated));
    }

    [Test]
    public async Task Update_WhenNotFound_ReturnsNotFound()
    {
        _repository.Setup(r => r.UpdateAsync(It.IsAny<AttributeType>())).ReturnsAsync((AttributeType?)null);

        var result = await _controller.Update(Guid.NewGuid(), new AttributeTypeRequest("Population", null));

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Update_WhenRepositoryThrowsValidationException_ReturnsProblem400()
    {
        _repository.Setup(r => r.UpdateAsync(It.IsAny<AttributeType>()))
            .ThrowsAsync(new ValidationException("An AttributeType with that name already exists."));

        var result = await _controller.Update(Guid.NewGuid(), new AttributeTypeRequest("Population", null));

        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult!.StatusCode, Is.EqualTo(400));
        var problem = objectResult.Value as ProblemDetails;
        Assert.That(problem, Is.Not.Null);
        Assert.That(problem!.Detail, Is.EqualTo("An AttributeType with that name already exists."));
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
            .ThrowsAsync(new ValidationException("The AttributeType cannot be deleted because it is referenced by one or more KnowledgeNodeAttributes."));

        var result = await _controller.Delete(Guid.NewGuid());

        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult!.StatusCode, Is.EqualTo(400));
        var problem = objectResult.Value as ProblemDetails;
        Assert.That(problem, Is.Not.Null);
        Assert.That(problem!.Detail, Is.EqualTo("The AttributeType cannot be deleted because it is referenced by one or more KnowledgeNodeAttributes."));
    }
}
