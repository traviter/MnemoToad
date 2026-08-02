using Microsoft.EntityFrameworkCore;
using MnemoToad.Data.Entities;

namespace MnemoToad.Data.Repositories;

public class NodeTypeRepository : INodeTypeRepository
{
    private readonly AppDbContext _db;

    public NodeTypeRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<NodeType>> GetAllAsync() =>
        _db.NodeType.OrderBy(n => n.Name).ToListAsync();

    public async Task<NodeType?> GetByIdAsync(Guid id) =>
        await _db.NodeType.FindAsync(id);

    public Task<bool> ExistsWithNameAsync(string name, Guid? excludingId = null) =>
        _db.NodeType.AnyAsync(n => n.Name == name && (excludingId == null || n.Id != excludingId));

    public Task AddAsync(NodeType nodeType)
    {
        _db.NodeType.Add(nodeType);
        return Task.CompletedTask;
    }

    public void Remove(NodeType nodeType) => _db.NodeType.Remove(nodeType);

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}
