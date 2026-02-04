namespace Frontend.Views;

public class DeploymentsView : ViewBase
{
  public override object? Build()
  {
    var client = UseService<IClientProvider>();

    var deployments = new[]
    {
            new { Name = "Prod-ResNet", Status = "Active", DeployedAt = "2024-02-01 10:30", Health = "Healthy", Traffic = "45%" },
            new { Name = "Staging-YOLO", Status = "Active", DeployedAt = "2024-02-03 14:20", Health = "Healthy", Traffic = "5%" },
            new { Name = "Archived-BERT", Status = "Archived", DeployedAt = "2024-01-15 09:00", Health = "N/A", Traffic = "0%" }
        };

    var table = new Table()
        | new TableRow()
            | new TableCell(Text.Block("Deployment Name").Bold())
            | new TableCell(Text.Block("Status").Bold())
            | new TableCell(Text.Block("Health").Bold())
            | new TableCell(Text.Block("Traffic").Bold())
            | new TableCell(Text.Block("Deployed At").Bold())
            | new TableCell(Text.Block("Actions").Bold());

    foreach (var dep in deployments)
    {
      table |= new TableRow()
          | new TableCell(dep.Name)
          | new TableCell(new Badge(dep.Status).Info())
          | new TableCell(new Badge(dep.Health).Success())
          | new TableCell(dep.Traffic)
          | new TableCell(dep.DeployedAt)
          | new TableCell(
              Layout.Horizontal().Gap(2)
                  | new Button("Logs", () => client.Toast($"Logs for {dep.Name}")).Secondary().Small()
                  | new Button("Settings", () => client.Toast($"Settings for {dep.Name}")).Secondary().Small()
          );
    }

    return Layout.Vertical().Gap(8).Padding(8)
        | Text.H1("Deployments")
        | new Card(table);
  }
}
