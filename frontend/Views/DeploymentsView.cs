namespace Frontend.Views;



public class DeploymentsView : ViewBase
{
  private readonly List<Run> _runs;

  public DeploymentsView(List<Run> runs)
  {
    _runs = runs;
  }

  public override object? Build()
  {
    var client = UseService<IClientProvider>();

    var currentRuns = _runs;

    // Eligible for deployment: Staging (1) and Production (2)
    var deploymentRegistry = currentRuns.Where(r => r.Stage == 1 || r.Stage == 2).OrderByDescending(r => r.Stage).ThenByDescending(r => r.Id).ToList();




    var table = new Table()
        | new TableRow()
            | new TableCell(Text.Block("Model Name").Bold())
            | new TableCell(Text.Block("Scenario").Bold())
            | new TableCell(Text.Block("Deployment Status").Bold())
            | new TableCell(Text.Block("Health").Bold())
            | new TableCell(Text.Block("Accuracy/R2").Bold())
            | new TableCell(Text.Block("Deployed At").Bold())
            | new TableCell(Text.Block("Actions").Bold());

    foreach (var run in deploymentRegistry)
    {
      bool isProduction = run.Stage == 2;

      table |= new TableRow()
          | new TableCell(run.Name)
          | new TableCell(new Badge(run.Scenario).Info())
          | new TableCell(isProduction ? new Badge("Online").Success() : new Badge("Offline").Secondary())
          | new TableCell(isProduction ? new Badge("Healthy").Success() : new Badge("Staging"))
          | new TableCell(run.Scenario == "Classification" ? run.Accuracy.ToString("P1") : run.RSquared.ToString("F3"))
          | new TableCell(isProduction ? run.CreatedAt : "N/A")
          | new TableCell(
              Layout.Horizontal().Gap(2)
                  | (isProduction ?
                      new Button("Logs").Secondary().Small().WithSheet(
                          () => new DeploymentLogsView(run),
                          title: $"Deployment Logs: {run.Name}",
                          description: $"History of deployments for {run.Name}",
                          width: Size.Fraction(1 / 2f)
                      ) :
                      new Button("Push to Production", async () =>
                      {
                        using var httpClient = new System.Net.Http.HttpClient();
                        httpClient.BaseAddress = new Uri("http://localhost:5153/");
                        var response = await httpClient.PostAsync($"api/runs/{run.Id}/promote", null);
                        if (response.IsSuccessStatusCode)
                        {
                          client.Toast($"Model {run.Name} is now LIVE!");
                        }
                      }).Primary().Small().Icon(Icons.Zap))
                  | (isProduction ?
                      new Button("Rollback", async () =>
                      {
                        using var httpClient = new System.Net.Http.HttpClient();
                        httpClient.BaseAddress = new Uri("http://localhost:5153/");
                        var response = await httpClient.PostAsync($"api/runs/{run.Id}/rollback", null);
                        if (response.IsSuccessStatusCode)
                        {
                          client.Toast($"Model {run.Name} rolled back to Staging.");
                        }
                      }).Secondary().Small().Destructive() :
                      null)
          );
    }

    return Layout.Vertical().Gap(8).Padding(8)
        | (Layout.Grid().Columns(2)
            | (Layout.Horizontal().Align(Align.Left).Gap(4).Align(Align.Left)
                | Text.H1("Model Deployment Registry")
                | Text.P($"{deploymentRegistry.Count} models available").Muted())
            | (Layout.Horizontal().Align(Align.Right)
                | new Spacer()))
        | new Card(table);
  }
}
