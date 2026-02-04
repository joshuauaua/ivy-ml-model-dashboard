using System;

namespace IvyMLDashboard.Models
{
  public class Deployment
  {
    public int Id { get; set; }
    public string RunName { get; set; } = string.Empty;
    public DeploymentStatus Status { get; set; }
    public DateTime DeployedAt { get; set; } = DateTime.UtcNow;
    public DeploymentHealth Health { get; set; }
  }
}
