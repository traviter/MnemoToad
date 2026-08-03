using Microsoft.EntityFrameworkCore;
using MnemoToad.Data;

namespace MnemoToad.Tests.TestSupport;

internal static class InMemoryAppDbContext
{
    public static AppDbContext Create() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
