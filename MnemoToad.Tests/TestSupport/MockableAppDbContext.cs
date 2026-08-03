using Microsoft.EntityFrameworkCore;
using Moq;
using MnemoToad.Data;
using MnemoToad.Data.Entities;

namespace MnemoToad.Tests.TestSupport;

internal sealed class MockableAppDbContext : IAppDbContext
{
    private readonly IAppDbContext _wrapped;
    private readonly Mock<IAppDbContext> _mock;

    public MockableAppDbContext(IAppDbContext? wrapped = null)
    {
        _wrapped = wrapped ?? InMemoryAppDbContext.Create();
        _mock = new Mock<IAppDbContext>();
        _mock.Setup(db => db.SaveChangesAsync()).Returns(() => _wrapped.SaveChangesAsync());
    }

    public DbSet<NodeType> NodeType => _wrapped.NodeType;
    public DbSet<KnowledgeNode> KnowledgeNode => _wrapped.KnowledgeNode;
    public DbSet<RelationshipType> RelationshipType => _wrapped.RelationshipType;
    public DbSet<KnowledgeRelation> KnowledgeRelation => _wrapped.KnowledgeRelation;
    public Task<int> SaveChangesAsync() => _mock.Object.SaveChangesAsync();

    public MockableAppDbContext ThrowOnSaveChanges(Exception exception)
    {
        _mock.Setup(db => db.SaveChangesAsync()).ThrowsAsync(exception);
        return this;
    }

    public void Dispose() => _wrapped.Dispose();
}
