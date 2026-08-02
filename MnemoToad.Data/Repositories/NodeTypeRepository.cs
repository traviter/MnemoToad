using Microsoft.EntityFrameworkCore;
using MnemoToad.Data.Entities;
using Npgsql;
using System.ComponentModel.DataAnnotations;

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

    public Task AddAsync(NodeType nodeType)
    {
        _db.NodeType.Add(nodeType);
        return Task.CompletedTask;
    }

    public void Remove(NodeType nodeType) => _db.NodeType.Remove(nodeType);

    // Name uniqueness and the FK from knowledge_node blocking delete are both enforced by DB
    // constraints rather than pre-flighted before writing, so a violation surfaces only here.
    // Translate just those two known constraint failures into a ValidationException (the service
    // layer maps that to a 400); anything else (e.g. the DB being unreachable) propagates unhandled
    // and becomes a 500.
    public async Task SaveChangesAsync()
    {
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation
        })
        {
            throw new ValidationException("A NodeType with that name already exists.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.ForeignKeyViolation
        })
        {
            throw new ValidationException("The NodeType cannot be deleted because it is referenced by one or more KnowledgeNodes.");
        }
    }
}
