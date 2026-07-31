using Microsoft.EntityFrameworkCore;
using MnemoToad.Api.Models;

namespace MnemoToad.Api.Data
{

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Country> Country => Set<Country>();
    }
}
