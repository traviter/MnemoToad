using MnemoToad.Knowledge.Data.Entities;

namespace MnemoToad.Knowledge.Data.Repositories;

public interface IAttributeTypeRepository
{
    Task<List<AttributeType>> GetAllAsync();
    Task<AttributeType?> GetByIdAsync(Guid id);
    Task<AttributeType> CreateAsync(AttributeType attributeType);
    Task<AttributeType?> UpdateAsync(AttributeType attributeType);
    Task<bool> DeleteAsync(Guid id);
}
