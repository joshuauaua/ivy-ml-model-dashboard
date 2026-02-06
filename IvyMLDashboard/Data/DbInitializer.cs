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
        new Run { Name = "Sentiment-v1", Scenario = "Classification", Stage = RunStage.Production, Accuracy = 0.89, CreatedAt = DateTime.UtcNow.AddDays(-5) },
        new Run { Name = "Sentiment-v2-Beta", Scenario = "Classification", Stage = RunStage.Staging, Accuracy = 0.91, CreatedAt = DateTime.UtcNow.AddDays(-2) },
        new Run { Name = "SpamDetector-X", Scenario = "Classification", Stage = RunStage.Staging, Accuracy = 0.85, CreatedAt = DateTime.UtcNow.AddHours(-5) }
      };
      context.Runs.AddRange(runs);

      var deployments = new Deployment[]
      {
        new Deployment { RunName = "Sentiment-v1", Status = DeploymentStatus.Production, Health = DeploymentHealth.Active, DeployedAt = DateTime.UtcNow.AddDays(-1) },
        new Deployment { RunName = "OldModel-Alpha", Status = DeploymentStatus.Archived, Health = DeploymentHealth.Active, DeployedAt = DateTime.UtcNow.AddDays(-10) },
        new Deployment { RunName = "TestRun-42", Status = DeploymentStatus.Archived, Health = DeploymentHealth.Inactive, DeployedAt = DateTime.UtcNow.AddDays(-15) },
        new Deployment { RunName = "Classifier-101", Status = DeploymentStatus.Archived, Health = DeploymentHealth.Active, DeployedAt = DateTime.UtcNow.AddDays(-20) },
        new Deployment { RunName = "Initial-Prod", Status = DeploymentStatus.Archived, Health = DeploymentHealth.Active, DeployedAt = DateTime.UtcNow.AddMonths(-1) }
      };

      context.Deployments.AddRange(deployments);
      context.SaveChanges();
    }
  }
}
