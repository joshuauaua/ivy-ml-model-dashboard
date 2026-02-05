using Frontend.Views;

namespace Frontend.Apps;

[App("dashboard")]
public class MLModelDashboardApp : ViewBase
{
  public override object? Build()
  {
    var client = UseService<IClientProvider>();
    var selectedItem = UseState("workspace");
    var selectedRunForMetrics = UseState("");

    // Sync with the same "runs" query used in RunsView for real-time updates
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

    // Sort by creation time to show the most recent runs in the sidebar
    var recentRunItems = currentRuns
                          .OrderByDescending(r => r.Id)
                          .Take(6)
                          .Select(r => MenuItem.Default(r.Name).Tag($"run:{r.Name}"))
                          .ToArray();

    // Recent Deployments = Promoted to Prod (Stage 2)
    var recentDeploymentItems = currentRuns
                                  .Where(r => r.Stage == 2)
                                  .OrderByDescending(r => r.Id)
                                  .Take(3)
                                  .Select(r => MenuItem.Default(r.Name).Tag("deployments")) // Route to deployments view
                                  .ToArray();

    MenuItem[] menuItems = new[]
    {

                MenuItem.Default("Options").Icon(Icons.Package).Children(
                    MenuItem.Default("Workspace").Tag("workspace"),
                    MenuItem.Default("Live").Tag("live"),
                    MenuItem.Default("Runs").Tag("runs"),
                    MenuItem.Default("Deployments").Tag("deployments"),
                    MenuItem.Default("Demo").Tag("demo")
                ),

                MenuItem.Default("Recent Runs").Icon(Icons.Cpu).Children(recentRunItems),
                MenuItem.Default("Recent Deployments").Icon(Icons.Table).Children(recentDeploymentItems)

        };

    var menu = new SidebarMenu(
        onSelect: evt =>
        {
          var tag = evt.Value?.ToString();
          if (tag == null) return;

          // Simple internal router based on tag patterns
          if (tag.StartsWith("run:"))
          {
            selectedItem.Set("runs");
            selectedRunForMetrics.Set(tag.Split(':')[1]);
          }
          else if (tag.StartsWith("dep-"))
          {
            selectedItem.Set("deployments");
            selectedRunForMetrics.Set("");
          }
          else
          {
            selectedItem.Set(tag);
            selectedRunForMetrics.Set("");
          }
        },
        items: menuItems
    );

    object dashboardContent = selectedItem.Value switch
    {
      "workspace" => new WorkspaceView(),
      "runs" when !string.IsNullOrEmpty(selectedRunForMetrics.Value) =>
          new RunMetricsView(selectedRunForMetrics.Value, () => selectedRunForMetrics.Set("")),
      "runs" => new RunsView(val => selectedRunForMetrics.Set(val)),
      "deployments" => new DeploymentsView(),
      "live" => new LiveView(),
      "demo" => new Frontend.Views.DemoView(),
      _ => new WorkspaceView()
    };

    return new SidebarLayout(
        mainContent: dashboardContent,
        sidebarContent: menu,
        sidebarHeader: Text.Lead("ML Model Dashboard")
    );
  }
}
