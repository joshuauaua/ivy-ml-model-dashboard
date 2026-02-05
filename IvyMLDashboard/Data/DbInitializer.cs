using Microsoft.EntityFrameworkCore;
using IvyMLDashboard.Models;

namespace IvyMLDashboard.Data
{
  public class DbInitializer
  {
    public static void Initialize(AppDbContext context)
    {
      context.Database.EnsureCreated();

      if (context.Runs.Any())
      {
        return;   // DB has been seeded
      }

      var runs = new Run[] { };
      var deployments = new Deployment[] { };

      context.Deployments.AddRange(deployments);
      context.SaveChanges();
    }
  }
}
