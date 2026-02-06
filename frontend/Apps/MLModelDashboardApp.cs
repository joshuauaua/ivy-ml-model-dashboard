using Frontend.Views;

namespace Frontend.Apps;

public class Deployment
{
  public int Id { get; set; }
  public string RunName { get; set; } = string.Empty;
  public int Status { get; set; }
  public string DeployedAt { get; set; } = string.Empty;
  public int Health { get; set; }
}

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

    // Deployments Query
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
      new QueryOptions { RefreshInterval = TimeSpan.FromSeconds(3) }
    );

    var currentRuns = runsQuery.Value ?? new List<Run>();
    var currentDeployments = deploymentsQuery.Value ?? new List<Deployment>();

    // Sort by creation time to show the most recent runs in the sidebar
    var recentRunItems = currentRuns
                          .OrderByDescending(r => r.Id)
                          .Take(6)
                          .Select(r => MenuItem.Default(r.Name).Tag($"run:{r.Name}"))
                          .ToArray();

    // Recent Deployments = From the Deployments table
    var recentDeploymentItems = currentDeployments
                                  .OrderByDescending(d => d.DeployedAt)
                                  .Take(5)
                                  .Select(d => MenuItem.Default(d.RunName).Tag("deployments")) // Route to deployments view
                                  .ToArray();

    MenuItem[] menuItems = new[]
    {

                MenuItem.Default("").Icon(Icons.Package).Children(
                    MenuItem.Default("Workspace").Tag("workspace"),
                    MenuItem.Default("Runs").Tag("runs"),
                    MenuItem.Default("Deployments").Tag("deployments"),
                    MenuItem.Default("Live").Tag("live"),
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
      "workspace" => new WorkspaceView(val => { selectedRunForMetrics.Set(val); selectedItem.Set("runs"); }),
      "runs" when !string.IsNullOrEmpty(selectedRunForMetrics.Value) =>
          new RunMetricsView(selectedRunForMetrics.Value, () => selectedRunForMetrics.Set("")),
      "runs" => new RunsView(val => selectedRunForMetrics.Set(val)),
      "deployments" => new DeploymentsView(),
      "live" => new LiveView(),
      "demo" => new Frontend.Views.DemoView(),
      _ => new WorkspaceView(val => { selectedRunForMetrics.Set(val); selectedItem.Set("runs"); })
    };

    return new SidebarLayout(
        mainContent: dashboardContent,
        sidebarContent: menu,
        sidebarHeader: Text.Lead("ML Model Dashboard")
    );
  }
}
