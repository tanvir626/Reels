using Reels.Models;
using Microsoft.EntityFrameworkCore;
using Reels.Models;

namespace Reels.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Reel> Reels => Set<Reel>();
}
