using MnemoToad.Data.Entities;
using MnemoToad.Data.Repositories;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Api.Services
{
    public class NodeTypeService : INodeTypeService
    {
        private readonly INodeTypeRepository _repository;
        private readonly IKnowledgeNodeRepository _knowledgeNodeRepository;

        public NodeTypeService(INodeTypeRepository repository, IKnowledgeNodeRepository knowledgeNodeRepository)
        {
            _repository = repository;
            _knowledgeNodeRepository = knowledgeNodeRepository;
        }

        public Task<List<NodeType>> GetAllAsync() => _repository.GetAllAsync();

        public Task<NodeType?> GetByIdAsync(Guid id) => _repository.GetByIdAsync(id);

        public async Task<NodeType> CreateAsync(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Name is required.");

            if (await _repository.ExistsWithNameAsync(name))
                throw new ValidationException($"A NodeType named '{name}' already exists.");

            var nodeType = new NodeType { Id = Guid.NewGuid(), Name = name, Description = description };
            await _repository.AddAsync(nodeType);
            await _repository.SaveChangesAsync();
            return nodeType;
        }

        public async Task<NodeType?> UpdateAsync(Guid id, string name, string? description)
        {
            var nodeType = await _repository.GetByIdAsync(id);
            if (nodeType is null) return null;

            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Name is required.");

            if (await _repository.ExistsWithNameAsync(name, id))
                throw new ValidationException($"A NodeType named '{name}' already exists.");

            nodeType.Name = name;
            nodeType.Description = description;
            await _repository.SaveChangesAsync();
            return nodeType;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var nodeType = await _repository.GetByIdAsync(id);
            if (nodeType is null) return false;

            if (await _knowledgeNodeRepository.ExistsByNodeTypeIdAsync(id))
                throw new ValidationException($"NodeType '{nodeType.Name}' cannot be deleted because it is referenced by one or more KnowledgeNodes.");

            _repository.Remove(nodeType);
            await _repository.SaveChangesAsync();
            return true;
        }
    }
}
