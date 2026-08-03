using Microsoft.EntityFrameworkCore;
using MnemoToad.Data.Entities;

namespace MnemoToad.Data;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<NodeType> NodeType => Set<NodeType>();
    public DbSet<KnowledgeNode> KnowledgeNode => Set<KnowledgeNode>();
    public DbSet<RelationshipType> RelationshipType => Set<RelationshipType>();
    public DbSet<KnowledgeRelation> KnowledgeRelation => Set<KnowledgeRelation>();

    public Task<int> SaveChangesAsync() => SaveChangesAsync(CancellationToken.None);
}
