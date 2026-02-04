namespace IvyMLDashboard.Models
{
    public enum RunStage
    {
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
