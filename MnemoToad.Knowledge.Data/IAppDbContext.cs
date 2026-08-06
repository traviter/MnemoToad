using Microsoft.EntityFrameworkCore;
using MnemoToad.Knowledge.Data.Entities;

namespace MnemoToad.Knowledge.Data;

public interface IAppDbContext : IDisposable
{
    DbSet<NodeType> NodeType { get; }
    DbSet<KnowledgeNode> KnowledgeNode { get; }
    DbSet<RelationshipType> RelationshipType { get; }
    DbSet<KnowledgeRelation> KnowledgeRelation { get; }
    DbSet<KnowledgeNodeAttribute> KnowledgeNodeAttribute { get; }

    Task<int> SaveChangesAsync();
    Task<int> ExecuteDeleteAsync<TEntity>(IQueryable<TEntity> query) where TEntity : class;
}
