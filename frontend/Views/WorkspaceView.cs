namespace Frontend.Views;

public class WorkspaceView : ViewBase
{
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

    var metricCard = (string label, string value, string trend, bool isSuccess = true) =>
    {
      var badge = new Badge(trend).Small();
      if (isSuccess) badge.Success(); else badge.Warning();

      return new Card(
          Layout.Vertical().Gap(2).Padding(4)
              | Text.Block(label).Small().Muted()
              | Text.H2(value)
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
        | Text.H1("Workspace Overview")

        | new Card(
            Layout.Grid().Columns(3).Gap(4).Padding(6)
                | metricCard("Active Runs", activeRuns.ToString(), "Currently Training", activeRuns > 0)
                | metricCard("Healthy Deployments", healthyDeployments.ToString(), healthyDeployments > 0 ? "Stable" : "No active model", healthyDeployments > 0)
                | metricCard("System Status", systemStatus, "All systems optimal", healthyDeployments > 0)
        ).Header("Key Metrics")

        | Text.H2("Global Activity Feed")
        | new Card(
            Layout.Vertical().Gap(3).Padding(4)
                | activities.Select(a =>
                    Layout.Horizontal().Gap(2).Align(Align.Left)
                        | Text.Block("•").Muted()
                        | Text.Block(a.Text).Small()
                        | new Spacer()
                        | Text.Block(a.Time.ToShortTimeString()).Small().Muted()
                  ).ToArray()
        ).Header("Activity Log");
  }
}
