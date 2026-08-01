using global::MnemoToad.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Api.Data
{
    public class NodeTypeService
    {
        private readonly AppDbContext _db;

        public NodeTypeService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<NodeType>> GetAllAsync() =>
            await _db.NodeType.OrderBy(n => n.Name).ToListAsync();

        public async Task<NodeType?> GetByIdAsync(Guid id) =>
            await _db.NodeType.FindAsync(id);

        public async Task<NodeType> CreateAsync(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Name is required.");

            if (await _db.NodeType.AnyAsync(n => n.Name == name))
                throw new ValidationException($"A NodeType named '{name}' already exists.");

            var nodeType = new NodeType { Id = Guid.NewGuid(), Name = name, Description = description };
            _db.NodeType.Add(nodeType);
            await _db.SaveChangesAsync();
            return nodeType;
        }

        public async Task<NodeType?> UpdateAsync(Guid id, string name, string? description)
        {
            var nodeType = await _db.NodeType.FindAsync(id);
            if (nodeType is null) return null;

            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Name is required.");

            if (await _db.NodeType.AnyAsync(n => n.Name == name && n.Id != id))
                throw new ValidationException($"A NodeType named '{name}' already exists.");

            nodeType.Name = name;
            nodeType.Description = description;
            await _db.SaveChangesAsync();
            return nodeType;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var nodeType = await _db.NodeType.FindAsync(id);
            if (nodeType is null) return false;

            // TODO: once KnowledgeNode exists, check for references here
            // and throw/return a conflict instead of deleting.

            _db.NodeType.Remove(nodeType);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}