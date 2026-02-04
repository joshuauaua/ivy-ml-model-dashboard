using Microsoft.EntityFrameworkCore;
using IvyMLDashboard.Models;

namespace IvyMLDashboard.Data
{
  public class AppDbContext : DbContext
  {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<IvyMLDashboard.Models.Run> Runs { get; set; } = default!;
    public DbSet<Deployment> Deployments { get; set; } = default!;
  }
}
