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

    var runs = UseState(new List<Run>());

    async Task fetchRecentRuns()
    {
      using var httpClient = new System.Net.Http.HttpClient();
      httpClient.BaseAddress = new Uri("http://localhost:5153/");
      try
      {
        var response = await httpClient.GetAsync("api/runs");
        if (response.IsSuccessStatusCode)
        {
          var responseJson = await response.Content.ReadAsStringAsync();
          var fetchedRuns = System.Text.Json.JsonSerializer.Deserialize<List<Run>>(responseJson, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
          if (fetchedRuns != null)
          {
            runs.Set(fetchedRuns.OrderByDescending(r => r.CreatedAt).Take(6).ToList());
          }
        }
      }
      catch { }
    }

    UseEffect(async () =>
    {
      await fetchRecentRuns();
    }, EffectTrigger.OnMount());

    // Optional: Refresh navigation periodically if there are active trainings
    UseEffect(async () =>
    {
      await Task.Delay(10000);
      await fetchRecentRuns();
    }, EffectTrigger.OnStateChange(runs));

    // We'll use the tags directly as state keys to simplify the mapping
    var recentRunItems = runs.Value.Select(r => MenuItem.Default(r.Name).Tag($"run:{r.Name}")).ToArray();

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
                MenuItem.Default("Recent Deployments").Icon(Icons.Table).Children(
                    MenuItem.Default("Prod-ResNet").Tag("dep-1"),
                    MenuItem.Default("Staging-YOLO").Tag("dep-2")
                )

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
