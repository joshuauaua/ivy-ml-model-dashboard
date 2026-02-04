using System;

namespace IvyMLDashboard.Models
{
    public class Run
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty; // Store as comma-separated or similar for simplicity in this demo
        public string Owner { get; set; } = string.Empty;
        public RunStage Stage { get; set; }
        public double Accuracy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
