namespace Frontend.Views;

public class DeploymentLogsView : ViewBase
{
  private readonly Run _run;

  public DeploymentLogsView(Run run)
  {
    _run = run;
  }

  public override object? Build()
  {
    // Fetch Deployments for Logs
    var deploymentsQuery = UseQuery<List<Deployment>, string>(
      "deployments",
      async (key, ct) =>
      {
        using var httpClient = new System.Net.Http.HttpClient();
        httpClient.BaseAddress = new Uri("http://localhost:5153/");
        var response = await httpClient.GetAsync("api/deployments", ct);
        response.EnsureSuccessStatusCode();
        var responseJson = await response.Content.ReadAsStringAsync(ct);
        return System.Text.Json.JsonSerializer.Deserialize<List<Deployment>>(responseJson, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Deployment>();
      },
      new QueryOptions { RefreshInterval = TimeSpan.FromSeconds(5) }
    );

    var allDeployments = deploymentsQuery.Value ?? new List<Deployment>();
    var runDeployments = allDeployments.Where(d => d.RunName == _run.Name).OrderByDescending(d => d.DeployedAt).ToList();

    return new Card(
        Layout.Vertical().Gap(4).Padding(6)
            | new Table()
                | new TableRow()
                    | new TableCell(Text.Block("Model Name").Bold())
                    | new TableCell(Text.Block("Accuracy").Bold())
                    | new TableCell(Text.Block("Deployed At").Bold())
                | (runDeployments.Select(d => new TableRow()
                        | new TableCell(d.RunName)
                        | new TableCell(_run.Scenario == "Classification" ? _run.Accuracy.ToString("P1") : _run.RSquared.ToString("F3"))
                        | new TableCell(d.DeployedAt.ToString("g"))
                    ))
    );
  }
}
