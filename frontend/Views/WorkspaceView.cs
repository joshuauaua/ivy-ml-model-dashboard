namespace Frontend.Views;

public class WorkspaceView : ViewBase
{
  private readonly Action<string> _onViewMetrics;

  public WorkspaceView(Action<string> onViewMetrics)
  {
    _onViewMetrics = onViewMetrics;
  }

  public override object? Build()
  {
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

    // Calculate Metrics
    int activeRuns = runs.Count(r => r.Stage == 0); // Training
    int healthyDeployments = runs.Count(r => r.Stage == 2); // Production
    string systemStatus = healthyDeployments > 0 ? "Online" : "Idle";

    // New Metrics
    int totalRuns = runs.Count;
    int totalOwners = runs.Select(r => r.Owner).Distinct().Count(o => !string.IsNullOrEmpty(o));
    double avgAccuracy = runs.Where(r => r.Scenario == "Classification").Select(r => r.Accuracy).DefaultIfEmpty(0).Average();
    int uniqueHyperparams = runs.Select(r => r.Hyperparameters).Distinct().Count();

    var allTags = runs.SelectMany(r => r.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).ToList();
    string mostFrequentTag = allTags.GroupBy(t => t).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault() ?? "None";
    double avgTagsPerRun = totalRuns > 0 ? (double)allTags.Count / totalRuns : 0;

    var metricCard = (string label, string value, string trend, bool isSuccess = true) =>
    {
      var badge = new Badge(trend).Small();
      if (isSuccess) badge.Success(); else badge.Warning();

      return new Card(
          Layout.Vertical().Gap(1).Padding(2).Align(Align.Center)
              | Text.Block(label).Small().Muted()
              | Text.H3(value)
              | badge
      );
    };

    // Generate Activity Log
    var activities = runs
        .SelectMany(r =>
        {
          var list = new List<(DateTime Time, string Text)>();
          if (DateTime.TryParse(r.CreatedAt, out var dt))
          {
            list.Add((dt, $"Experiment '{r.Name}' initialized by {r.Owner}"));
          }
          if (r.Stage == 2) // Production
          {
            list.Add((DateTime.UtcNow, $"Run '{r.Name}' promoted to Production"));
          }
          return list;
        })
        .OrderByDescending(a => a.Time)
        .Take(5)
        .ToList();

    return Layout.Vertical().Gap(8).Padding(8)
        | (Layout.Grid().Columns(2)
            | (Layout.Horizontal().Align(Align.Left).Gap(4).Align(Align.Center)
                | Text.H1("Workspace Overview")
                | Text.H3("Dashboard").Muted())
            | (Layout.Horizontal().Align(Align.Right)
                | new Spacer()))

        | (Layout.Grid().Columns(3).Gap(4)
            | metricCard("Total Runs", totalRuns.ToString(), "All Experiments")
            | metricCard("Total Owners", totalOwners.ToString(), "Unique Collaborators")
            | metricCard("Avg Accuracy", avgAccuracy.ToString("P1"), "Classification")
            | metricCard("Unique Hyperparams", uniqueHyperparams.ToString(), "Distinct Configs")
            | metricCard("Most Frequent Tag", mostFrequentTag, "Trending Area")
            | metricCard("Avg Tags per Run", avgTagsPerRun.ToString("F1"), "Metadata Density"))

        | (runs.OrderByDescending(r => r.Accuracy).FirstOrDefault() is var bestModel && bestModel != null ?


            new Card(
                Layout.Vertical().Gap(4).Padding(6).Width(Size.Full())
                    | (Layout.Horizontal().Gap(4).Align(Align.Center)
                        | new Badge("").Icon(Icons.Award).Warning()
                        | (Layout.Vertical().Gap(0)
                            | Text.H3("Best Performing Model")
                            | Text.Block(bestModel.Name).Small().Muted()))
                    | new Spacer().Height(1)
                    | (Layout.Grid().Columns(3).Gap(2).Padding(4)
                        | (Layout.Vertical().Align(Align.Right).Gap(4)
                            | Text.Block("Accuracy").Small().Muted()
                            | new Badge(bestModel.Accuracy.ToString("F2")).Success())
                        | (Layout.Vertical().Align(Align.Center).Gap(4)
                            | Text.Block("Owner").Small().Muted()
                            | (Layout.Horizontal().Gap(2).Align(Align.Center)
                                | new Badge(bestModel.Owner.Substring(0, Math.Min(2, bestModel.Owner.Length)).ToUpper()).Info()
                                | Text.Block(bestModel.Owner).Bold()))
                        | (Layout.Vertical().Align(Align.Left).Gap(4)
                            | Text.Block("Tags").Small().Muted()
                            | (Layout.Horizontal().Gap(1).Align(Align.Center).Wrap()
                                | bestModel.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                    .Select(t => new Badge(t).Info().Small()).ToArray())))
                        | new Spacer().Height(1)
                        | new Button("View Run Details", () => _onViewMetrics(bestModel.Name)).Primary().Width(Size.Full())
            ) : null)


        | new Card(
            Layout.Vertical().Gap(3).Padding(4)
                | Text.H3("Activity Log")
                | activities.Select(a =>
                    Layout.Horizontal().Gap(2).Align(Align.Left)
                        | Text.Block("-").Muted()
                        | Text.Block(a.Text).Medium()
                        | new Spacer()
                        | Text.Block(a.Time.ToShortTimeString()).Small().Muted()
                  ).ToArray()
        );
  }
}
