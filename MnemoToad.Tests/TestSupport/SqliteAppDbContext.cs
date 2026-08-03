using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MnemoToad.Data;

namespace MnemoToad.Tests.TestSupport;

internal static class SqliteAppDbContext
{
    public static (AppDbContext Context, SqliteConnection Connection) Create()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options);
        context.Database.EnsureCreated();

        return (context, connection);
    }
}
