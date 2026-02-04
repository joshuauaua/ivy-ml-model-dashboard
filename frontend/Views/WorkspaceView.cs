namespace Frontend.Views;

public class WorkspaceView : ViewBase
{
  public override object? Build()
  {
    var client = UseService<IClientProvider>();

    return Layout.Vertical().Gap(8).Padding(8)
        | Text.H1("Workspace Overview")
        | Layout.Grid().Columns(3).Gap(6)
            | new Card("Current active experiment sessions.")
                .Title("Active Runs")
                .HandleClick(_ => client.Toast("Viewing active runs..."))
                | Layout.Vertical().Align(Align.Center)
                    | Text.H2("12")
                    | new Badge("4 New Today").Primary()
            | new Card("Models serving production traffic.")
                .Title("Healthy Deployments")
                .HandleClick(_ => client.Toast("Checking deployment health..."))
                | Layout.Vertical().Align(Align.Center)
                    | Text.H2("8")
                    | new Badge("Stable").Success()
            | new Card("Overall platform availability.")
                .Title("System Status")
                .HandleClick(_ => client.Toast("Platform is operational."))
                | Layout.Vertical().Align(Align.Center)
                    | Text.H2("Online")
                    | new Badge("All Systems Go").Info()
        | Text.H2("Recent Activity")
        | new Card("Latest updates from the team.")
            .Title("Activity Log")
            | Layout.Vertical()
                | Text.Block("• ResNet-50-v1 deployed to Production")
                | Text.Block("• YOLOv8-Nano training completed")
                | Text.Block("• BERT-Base-Uncased accuracy improved to 0.92");
  }
}
