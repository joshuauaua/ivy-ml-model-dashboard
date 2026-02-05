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

    // Metadata Card Builder
    var kpiCard = (string label, string value, object icon) =>
        new Card(
            Layout.Horizontal().Gap(4).Align(Align.Center).Padding(4)
                | icon
                | Layout.Vertical().Gap(0)
                    | Text.Block(label).Small().Muted()
                    | Text.Block(value).Bold()
        );

    // Metric Card Builder
    var metricCard = (string label, string value, string trend) =>
        new Card(
            Layout.Vertical().Gap(2).Padding(4)
                | Text.Block(label).Small().Muted()
                | Text.H2(value)
                | new Badge(trend).Success().Small()
        );

    var metadataGrid = Layout.Grid().Columns(4).Gap(4)
        | kpiCard("Owner", run.Owner, Icons.User)
        | kpiCard("Tags", run.Tags, Icons.Tag)
        | kpiCard("Scenario", run.Scenario, Icons.Cpu)
        | kpiCard("Stage", run.Stage switch { 0 => "Training", 1 => "Staging", 2 => "Production", _ => "Unknown" }, Icons.Layers);

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

    // Optional visualization card based on scenario
    var visualizationCard = run.Scenario switch
    {
      "Classification" or "Image classification" => new Card().Header("Confusion Matrix")
          | Layout.Vertical().Align(Align.Center).Padding(4)
              | Text.P("Predicted").Small()
              | Layout.Horizontal().Gap(2)
                  | Layout.Vertical().Align(Align.Center) | Text.P("Actual").Small()
                  | Layout.Grid().Columns(2).Rows(2).Gap(2)
                      | (Layout.Vertical().Align(Align.Center).Padding(4) | new Card() | Text.H3("942") | Text.P("TP").Small())
                      | (Layout.Vertical().Align(Align.Center).Padding(4) | new Card() | Text.H3("58") | Text.P("FN").Small())
                      | (Layout.Vertical().Align(Align.Center).Padding(4) | new Card() | Text.H3("23") | Text.P("FP").Small())
                      | (Layout.Vertical().Align(Align.Center).Padding(4) | new Card() | Text.H3("877") | Text.P("TN").Small()),
      "Regression" or "Forecasting" => new Card().Header("Residuals Plot (Simulated)")
          | new AreaChart(new[] {
                new { X = 0.0, Y = 0.5 }, new { X = 1.0, Y = 0.3 }, new { X = 2.0, Y = 0.7 },
                new { X = 3.0, Y = 0.2 }, new { X = 4.0, Y = 0.4 }
            }).Height(200),
      _ => new Card().Header("Model Insights") | Text.Block("Standard evaluation completed for " + run.Scenario).Italic()
    };

    var hyperparameters = new Card().Header("Hyperparameters")
        | Layout.Vertical().Gap(2).Padding(4)
            | Text.Block(run.Hyperparameters).Small().Italic()
            | new Spacer().Height(10)
            | Text.P("ID: " + run.Id).Small().Muted();

    return Layout.Vertical().Gap(6).Padding(8)
        | header
        | metadataGrid
        | Layout.Grid().Columns(2).Gap(6)
            | mainMetrics
            | Layout.Vertical().Gap(6)
                | visualizationCard
                | hyperparameters;
  }
}
