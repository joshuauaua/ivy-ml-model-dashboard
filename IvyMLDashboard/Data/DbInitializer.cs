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

      var runs = new Run[]
      {
                new Run { Name = "ResNet-50-v1", Tags = "vision,classification", Owner = "Alice", Stage = RunStage.Production, Accuracy = 0.94, CreatedAt = DateTime.UtcNow.AddDays(-10) },
                new Run { Name = "BERT-Base-Uncased", Tags = "nlp,transformer", Owner = "Bob", Stage = RunStage.Staging, Accuracy = 0.89, CreatedAt = DateTime.UtcNow.AddDays(-5) },
                new Run { Name = "YOLOv8-Nano", Tags = "vision,detection", Owner = "Charlie", Stage = RunStage.Production, Accuracy = 0.91, CreatedAt = DateTime.UtcNow.AddDays(-2) },
                new Run { Name = "GPT-2-Large", Tags = "nlp,generation", Owner = "Alice", Stage = RunStage.Staging, Accuracy = 0.85, CreatedAt = DateTime.UtcNow.AddHours(-12) }
      };

      context.Runs.AddRange(runs);
      context.SaveChanges();

      var deployments = new Deployment[]
      {
                new Deployment { RunName = "ResNet-50-v1", Status = DeploymentStatus.Production, DeployedAt = DateTime.UtcNow.AddDays(-8), Health = DeploymentHealth.Active },
                new Deployment { RunName = "YOLOv8-Nano", Status = DeploymentStatus.Production, DeployedAt = DateTime.UtcNow.AddDays(-1), Health = DeploymentHealth.Active },
                new Deployment { RunName = "BERT-Base-Uncased", Status = DeploymentStatus.Archived, DeployedAt = DateTime.UtcNow.AddDays(-15), Health = DeploymentHealth.Inactive }
      };

      context.Deployments.AddRange(deployments);
      context.SaveChanges();
    }
  }
}
