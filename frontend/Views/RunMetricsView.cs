using Ivy.Charts;

namespace Frontend.Views;

public class RunMetricsView : ViewBase
{
  private readonly string _runName;
  private readonly Action _onBack;

  public RunMetricsView(string runName, Action onBack)
  {
    _runName = runName;
    _onBack = onBack;
  }

  public override object? Build()
  {
    var client = UseService<IClientProvider>();

    // Fetch all runs to maintain cache consistency with the sidebar and main table
    var runsQuery = UseQuery<List<Run>, string>(
      "runs",
      async (key, ct) =>
      {
        using var httpClient = new System.Net.Http.HttpClient();
        httpClient.BaseAddress = new Uri("http://localhost:5153/");
        var response = await httpClient.GetAsync("api/runs", ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        return System.Text.Json.JsonSerializer.Deserialize<List<Run>>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Run>();
      },
      new QueryOptions { RefreshInterval = TimeSpan.FromSeconds(3) }
    );

    var runs = runsQuery.Value ?? new List<Run>();
    var run = runs.FirstOrDefault(r => r.Name == _runName) ?? new Run { Name = _runName };

    var header = Layout.Horizontal().Align(Align.Center)
        | new Button("Back", _onBack).Secondary().Icon(Icons.ArrowLeft)
        | new Spacer()
        | Layout.Horizontal().Gap(2).Align(Align.Center)
            | Text.H2(_runName)
            | (run.Stage == 2 ? new Badge("Production").Success() : (run.Stage == 1 ? new Badge("Staging").Info() : new Badge("Training").Warning()))
        | new Spacer()
        | (run.Stage == 1 ? new Button("Promote to Prod", async () =>
        {
          using var httpClient = new System.Net.Http.HttpClient();
          httpClient.BaseAddress = new Uri("http://localhost:5153/");
          var response = await httpClient.PostAsync($"api/runs/{run.Id}/promote", null);
          if (response.IsSuccessStatusCode)
          {
            client.Toast($"Model {run.Name} is now LIVE!");
          }
        }).Primary().Icon(Icons.Zap) : null)
        | (run.Stage == 2 ? new Button("Rollback", async () =>
        {
          using var httpClient = new System.Net.Http.HttpClient();
          httpClient.BaseAddress = new Uri("http://localhost:5153/");
          var response = await httpClient.PostAsync($"api/runs/{run.Id}/rollback", null);
          if (response.IsSuccessStatusCode)
          {
            client.Toast($"{run.Name} rolled back to Staging.");
          }
        }).Secondary().Destructive() : null);


    // Metric Card Builder
    var metricCard = (string label, string value, string trend) =>
        new Card(
            Layout.Vertical().Gap(2).Padding(4)
                | Text.Block(label).Small().Muted()
                | Text.H2(value)
                | new Badge(trend).Success().Small()
        );

    var infoCard = (string label, string value) =>
        new Card(
            Layout.Vertical().Gap(1).Padding(4)
                | Text.Block(label).Small().Muted()
                | Text.H2(value)
        );

    var metadataGrid = Layout.Grid().Columns(3).Gap(4)
        | infoCard("Owner", run.Owner)
        | infoCard("Tags", run.Tags)
        | infoCard("Scenario", run.Scenario);

    // Dynamic Metrics Grid based on Scenario
    object metricsGrid;
    if (run.Scenario == "Regression" || run.Scenario == "Forecasting" || run.Scenario == "Recommendation")
    {
      metricsGrid = Layout.Grid().Columns(3).Gap(4)
          | metricCard("R-Squared (R2)", run.RSquared.ToString("F3"), "↑ 0.05")
          | metricCard("Mean Abs Error (MAE)", run.MeanAbsoluteError.ToString("F4"), "↓ 0.01")
          | metricCard("Mean Sq Error (MSE)", run.MeanSquaredError.ToString("F4"), "↓ 0.02")
          | metricCard("RMS Error (RMSE)", run.RootMeanSquaredError.ToString("F4"), "↓ 0.01")
          | metricCard("Loss Function", (run.MeanSquaredError * 0.9).ToString("F4"), "↓ 0.03") // Simulated
          | metricCard("Scenario Fit", "High", "Optimal");
    }
    else
    {
      metricsGrid = Layout.Grid().Columns(3).Gap(4)
          | metricCard("Accuracy", run.Accuracy.ToString("P1"), "↑ 1.2%")
          | metricCard("AUC ROC", run.AreaUnderRocCurve.ToString("F3"), "↑ 0.8%")
          | metricCard("F1 Score", run.F1Score.ToString("F3"), "↑ 1.5%")
          | metricCard("Precision", run.Precision.ToString("P1"), "↑ 0.4%")
          | metricCard("Recall", run.Recall.ToString("P1"), "↓ 0.2%")
          | metricCard("Log-Loss", run.LogLoss.ToString("F4"), "↓ 0.05");
    }

    var mainMetrics = new Card().Header("Performance Metrics")
        | Layout.Vertical().Padding(4)
            | metricsGrid;

    return Layout.Vertical().Gap(6).Padding(8)
        | header
        | metadataGrid
        | mainMetrics;
  }
}
