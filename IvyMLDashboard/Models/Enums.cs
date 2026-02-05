namespace IvyMLDashboard.Models
{
    public enum RunStage
    {
        Training,
        Staging,
        Production
    }

    public enum DeploymentStatus
    {
        Production,
        Archived
    }

    public enum DeploymentHealth
    {
        Active,
        Inactive
    }
}
