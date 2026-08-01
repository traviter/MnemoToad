using Microsoft.EntityFrameworkCore;
using MnemoToad.Data.Entities;

namespace MnemoToad.Data
{

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<NodeType> NodeType => Set<NodeType>();
    }
}
