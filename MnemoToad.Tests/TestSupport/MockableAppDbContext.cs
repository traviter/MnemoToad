using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using MnemoToad.Data;
using MnemoToad.Data.Entities;

namespace MnemoToad.Tests.TestSupport;

internal sealed class MockableAppDbContext : IAppDbContext
{
    private readonly IAppDbContext _wrapped;
    private readonly SqliteConnection? _connection;
    private readonly Mock<IAppDbContext> _mock;

    public MockableAppDbContext(IAppDbContext? wrapped = null)
    {
        if (wrapped is null)
        {
            (var context, _connection) = SqliteAppDbContext.Create();
            _wrapped = context;
        }
        else
        {
            _wrapped = wrapped;
        }

        _mock = new Mock<IAppDbContext>();
        _mock.Setup(db => db.SaveChangesAsync()).Returns(() => _wrapped.SaveChangesAsync());
        SetupDefaultExecuteDelete<NodeType>();
        SetupDefaultExecuteDelete<KnowledgeNode>();
        SetupDefaultExecuteDelete<RelationshipType>();
        SetupDefaultExecuteDelete<KnowledgeRelation>();
    }

    private void SetupDefaultExecuteDelete<TEntity>() where TEntity : class =>
        _mock.Setup(db => db.ExecuteDeleteAsync(It.IsAny<IQueryable<TEntity>>()))
            .Returns<IQueryable<TEntity>>(query => _wrapped.ExecuteDeleteAsync(query));

    public DbSet<NodeType> NodeType => _wrapped.NodeType;
    public DbSet<KnowledgeNode> KnowledgeNode => _wrapped.KnowledgeNode;
    public DbSet<RelationshipType> RelationshipType => _wrapped.RelationshipType;
    public DbSet<KnowledgeRelation> KnowledgeRelation => _wrapped.KnowledgeRelation;
    public Task<int> SaveChangesAsync() => _mock.Object.SaveChangesAsync();
    public Task<int> ExecuteDeleteAsync<TEntity>(IQueryable<TEntity> query) where TEntity : class =>
        _mock.Object.ExecuteDeleteAsync(query);

    public MockableAppDbContext ThrowOnSaveChanges(Exception exception)
    {
        _mock.Setup(db => db.SaveChangesAsync()).ThrowsAsync(exception);
        return this;
    }

    public MockableAppDbContext ThrowOnExecuteDelete<TEntity>(Exception exception) where TEntity : class
    {
        _mock.Setup(db => db.ExecuteDeleteAsync(It.IsAny<IQueryable<TEntity>>())).ThrowsAsync(exception);
        return this;
    }

    public void Dispose()
    {
        _wrapped.Dispose();
        _connection?.Dispose();
    }
}
