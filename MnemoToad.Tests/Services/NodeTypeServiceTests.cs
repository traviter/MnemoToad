using Microsoft.EntityFrameworkCore;
using MnemoToad.Api.Services;
using MnemoToad.Data;
using System.ComponentModel.DataAnnotations;

namespace MnemoToad.Tests.Services
{
    public class NodeTypeServiceTests
    {
        private static AppDbContext CreateContext() =>
            new(new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        [Fact]
        public async Task CreateAsync_WithValidName_ReturnsCreatedNodeType()
        {
            var service = new NodeTypeService(CreateContext());

            var created = await service.CreateAsync("Person", "A human being");

            Assert.NotEqual(Guid.Empty, created.Id);
            Assert.Equal("Person", created.Name);
            Assert.Equal("A human being", created.Description);
        }

        [Fact]
        public async Task CreateAsync_WithBlankName_ThrowsValidationException()
        {
            var service = new NodeTypeService(CreateContext());

            await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync("  ", null));
        }

        [Fact]
        public async Task CreateAsync_WithDuplicateName_ThrowsValidationException()
        {
            var service = new NodeTypeService(CreateContext());
            await service.CreateAsync("Person", null);

            await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync("Person", null));
        }

        [Fact]
        public async Task GetAllAsync_ReturnsNodeTypesOrderedByName()
        {
            var service = new NodeTypeService(CreateContext());
            await service.CreateAsync("Zebra", null);
            await service.CreateAsync("Apple", null);

            var all = await service.GetAllAsync();

            Assert.Equal(["Apple", "Zebra"], all.Select(n => n.Name));
        }

        [Fact]
        public async Task GetByIdAsync_WhenExists_ReturnsNodeType()
        {
            var service = new NodeTypeService(CreateContext());
            var created = await service.CreateAsync("Person", null);

            var found = await service.GetByIdAsync(created.Id);

            Assert.NotNull(found);
            Assert.Equal("Person", found.Name);
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
        {
            var service = new NodeTypeService(CreateContext());

            var found = await service.GetByIdAsync(Guid.NewGuid());

            Assert.Null(found);
        }

        [Fact]
        public async Task UpdateAsync_WhenNotFound_ReturnsNull()
        {
            var service = new NodeTypeService(CreateContext());

            var updated = await service.UpdateAsync(Guid.NewGuid(), "Person", null);

            Assert.Null(updated);
        }

        [Fact]
        public async Task UpdateAsync_WithBlankName_ThrowsValidationException()
        {
            var service = new NodeTypeService(CreateContext());
            var created = await service.CreateAsync("Person", null);

            await Assert.ThrowsAsync<ValidationException>(() => service.UpdateAsync(created.Id, " ", null));
        }

        [Fact]
        public async Task UpdateAsync_WithAnotherNodeTypesName_ThrowsValidationException()
        {
            var service = new NodeTypeService(CreateContext());
            await service.CreateAsync("Person", null);
            var toUpdate = await service.CreateAsync("Place", null);

            await Assert.ThrowsAsync<ValidationException>(() => service.UpdateAsync(toUpdate.Id, "Person", null));
        }

        [Fact]
        public async Task UpdateAsync_WithOwnUnchangedName_Succeeds()
        {
            var service = new NodeTypeService(CreateContext());
            var created = await service.CreateAsync("Person", "Old description");

            var updated = await service.UpdateAsync(created.Id, "Person", "New description");

            Assert.NotNull(updated);
            Assert.Equal("New description", updated.Description);
        }

        [Fact]
        public async Task DeleteAsync_WhenExists_RemovesAndReturnsTrue()
        {
            var service = new NodeTypeService(CreateContext());
            var created = await service.CreateAsync("Person", null);

            var result = await service.DeleteAsync(created.Id);

            Assert.True(result);
            Assert.Null(await service.GetByIdAsync(created.Id));
        }

        [Fact]
        public async Task DeleteAsync_WhenNotFound_ReturnsFalse()
        {
            var service = new NodeTypeService(CreateContext());

            var result = await service.DeleteAsync(Guid.NewGuid());

            Assert.False(result);
        }
    }
}
