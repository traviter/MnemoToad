using Microsoft.AspNetCore.Mvc;
using Moq;
using MnemoToad.Knowledge.Api.Controllers;
using MnemoToad.Knowledge.Data.Repositories;
using NUnit.Framework;
using System.Text.Json.Nodes;

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
        var attributes = new Dictionary<string, JsonValue?> { ["isoCode"] = JsonValue.Create("FR") };
        _repository.Setup(r => r.GetByNodeIdAsync(nodeId)).ReturnsAsync(attributes);

        var result = await _controller.GetByNodeId(nodeId);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.SameAs(attributes));
    }
}
