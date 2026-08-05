using Microsoft.EntityFrameworkCore;
using MnemoToad.Knowledge.Data.Entities;
using Npgsql;

namespace MnemoToad.Knowledge.Data;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<NodeType> NodeType => Set<NodeType>();
    public DbSet<KnowledgeNode> KnowledgeNode => Set<KnowledgeNode>();
    public DbSet<RelationshipType> RelationshipType => Set<RelationshipType>();
    public DbSet<KnowledgeRelation> KnowledgeRelation => Set<KnowledgeRelation>();
    public DbSet<AttributeType> AttributeType => Set<AttributeType>();
    public DbSet<KnowledgeNodeAttribute> KnowledgeNodeAttribute => Set<KnowledgeNodeAttribute>();

    public Task<int> SaveChangesAsync() => SaveChangesAsync(CancellationToken.None);

    public async Task<int> ExecuteDeleteAsync<TEntity>(IQueryable<TEntity> query) where TEntity : class
    {
        try
        {
            return await query.ExecuteDeleteAsync();
        }
        catch (PostgresException ex)
        {
            throw new DbUpdateException(ex.Message, ex);
        }
    }
}
