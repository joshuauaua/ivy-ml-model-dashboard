using System;

namespace IvyMLDashboard.Models
{
  public class Run
  {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty; // Store as comma-separated or similar for simplicity in this demo
    public string Owner { get; set; } = string.Empty;
    public string Hyperparameters { get; set; } = "{}";
    public RunStage Stage { get; set; }
    public double Accuracy { get; set; }
    public double AreaUnderRocCurve { get; set; }
    public double F1Score { get; set; }
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double LogLoss { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  }
}
