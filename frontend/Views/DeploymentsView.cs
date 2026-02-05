namespace Frontend.Views;

public class DeploymentsView : ViewBase
{
  public override object? Build()
  {
    var client = UseService<IClientProvider>();

    // UseQuery for automatic updates, synced with other views
    var runsQuery = UseQuery<List<Run>, string>(
      "runs",
      async (key, ct) =>
      {
        using var httpClient = new System.Net.Http.HttpClient();
        httpClient.BaseAddress = new Uri("http://localhost:5153/");
        var response = await httpClient.GetAsync("api/runs", ct);
        response.EnsureSuccessStatusCode();
        var responseJson = await response.Content.ReadAsStringAsync(ct);
        return System.Text.Json.JsonSerializer.Deserialize<List<Run>>(responseJson, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Run>();
      },
      new QueryOptions { RefreshInterval = TimeSpan.FromSeconds(3) }
    );

    var currentRuns = runsQuery.Value ?? new List<Run>();

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
                      new Button("Logs", () => client.Toast($"Logs for {run.Name}")).Secondary().Small() :
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
        | (Layout.Horizontal().Align(Align.Center)
            | Text.H1("Model Deployment Registry")
            | new Spacer()
            | Text.P($"{deploymentRegistry.Count} models available").Muted())
        | new Card(table);
  }
}
