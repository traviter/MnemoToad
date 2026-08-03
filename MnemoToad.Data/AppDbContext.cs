using Microsoft.EntityFrameworkCore;
using MnemoToad.Data.Entities;
using Npgsql;

namespace MnemoToad.Data;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<NodeType> NodeType => Set<NodeType>();
    public DbSet<KnowledgeNode> KnowledgeNode => Set<KnowledgeNode>();
    public DbSet<RelationshipType> RelationshipType => Set<RelationshipType>();
    public DbSet<KnowledgeRelation> KnowledgeRelation => Set<KnowledgeRelation>();

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
