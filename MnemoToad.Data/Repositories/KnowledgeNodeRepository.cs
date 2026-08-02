using Microsoft.EntityFrameworkCore;
using MnemoToad.Data.Entities;

namespace MnemoToad.Data.Repositories;

public class KnowledgeNodeRepository : IKnowledgeNodeRepository
{
    private readonly AppDbContext _db;

    public KnowledgeNodeRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<KnowledgeNode>> GetAllAsync(Guid? nodeTypeId = null)
    {
        var query = _db.KnowledgeNode.AsQueryable();
        if (nodeTypeId is not null)
            query = query.Where(n => n.NodeTypeId == nodeTypeId);

        return query.OrderBy(n => n.CanonicalName).ToListAsync();
    }

    public async Task<KnowledgeNode?> GetByIdAsync(Guid id) =>
        await _db.KnowledgeNode.FindAsync(id);

    public Task<bool> ExistsByNodeTypeIdAsync(Guid nodeTypeId) =>
        _db.KnowledgeNode.AnyAsync(n => n.NodeTypeId == nodeTypeId);

    public Task AddAsync(KnowledgeNode knowledgeNode)
    {
        _db.KnowledgeNode.Add(knowledgeNode);
        return Task.CompletedTask;
    }

    public void Remove(KnowledgeNode knowledgeNode) => _db.KnowledgeNode.Remove(knowledgeNode);

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}
