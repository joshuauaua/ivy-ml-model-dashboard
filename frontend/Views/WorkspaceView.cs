namespace Frontend.Views;

public class WorkspaceView : ViewBase
{
  public override object? Build()
  {
    var metricCard = (string label, string value, string trend) =>
        new Card(
            Layout.Vertical().Gap(2).Padding(4)
                | Text.Block(label).Small().Muted()
                | Text.H2(value)
                | new Badge(trend).Success().Small()
        );

    return Layout.Vertical().Gap(8).Padding(8)
        | Text.H1("Workspace Overview")

        | new Card().Header("Key Metrics")
            | (Layout.Grid().Columns(3).Gap(4).Padding(6)
                | metricCard("Active Runs", "12", "↑ 2 this week")
                | metricCard("Healthy Deployments", "8", "Stable")
                | metricCard("System Status", "Online", "Optimal"))

        | Text.H2("Recent Activity")
        | new Card().Header("Activity Log")
            | Layout.Vertical().Gap(2).Padding(4)
                | Text.Block("• ResNet-50-v1 deployed to Production").Small()
                | Text.Block("• YOLOv8-Nano training completed").Small()
                | Text.Block("• BERT-Base-Uncased accuracy improved to 0.92").Small();
  }
}
