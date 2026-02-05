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

    // We'll use the tags directly as state keys to simplify the mapping
    MenuItem[] menuItems = new[]
    {

                MenuItem.Default("Options").Icon(Icons.Package).Children(
                    MenuItem.Default("Workspace").Tag("workspace"),
                    MenuItem.Default("Live").Tag("live"),
                    MenuItem.Default("Runs").Tag("runs"),
                    MenuItem.Default("Deployments").Tag("deployments"),
                    MenuItem.Default("Demo").Tag("demo")
                ),

                MenuItem.Default("Recent Runs").Icon(Icons.Cpu).Children(
                    MenuItem.Default("ResNet-50-v1").Tag("run:ResNet-50-v1"),
                    MenuItem.Default("BERT-Base").Tag("run:BERT-Base")
                ),
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
