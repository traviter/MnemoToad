using Microsoft.EntityFrameworkCore;
using System.Text.Json.Nodes;

namespace MnemoToad.Knowledge.Data.Repositories;

public class KnowledgeNodeAttributeRepository : IKnowledgeNodeAttributeRepository
{
    private readonly IAppDbContext _db;

    public KnowledgeNodeAttributeRepository(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Dictionary<string, JsonValue?>> GetByNodeIdAsync(Guid knowledgeNodeId)
    {
        var rows = await _db.KnowledgeNodeAttribute.Where(a => a.KnowledgeNodeId == knowledgeNodeId).ToListAsync();
        return rows.ToDictionary(r => r.Key, r => r.Value);
    }
}
