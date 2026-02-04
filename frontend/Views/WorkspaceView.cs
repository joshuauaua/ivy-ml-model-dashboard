namespace Frontend.Views;

public class WorkspaceView : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical().Gap(8).Padding(8)
            | Text.H1("ML Model Dashboard")
            | Text.Lead("Welcome back. Here is your system overview.")
            | Layout.Grid().Columns(3).Gap(6)
                | new Card()
                    .Header("Active Runs")
                    | Layout.Vertical().Align(Align.Center)
                        | Text.H2("12")
                        | new Badge("4 New Today").Primary()
                | new Card()
                    .Header("Healthy Deployments")
                    | Layout.Vertical().Align(Align.Center)
                        | Text.H2("8")
                        | new Badge("Stable").Success()
                | new Card()
                    .Header("System Status")
                    | Layout.Vertical().Align(Align.Center)
                        | Text.H2("Online")
                        | new Badge("All Systems Go").Info()
            | Text.H2("Recent Activity")
            | new Card()
                | Layout.Vertical()
                    | Text.Block("• ResNet-50-v1 deployed to Production")
                    | Text.Block("• YOLOv8-Nano training completed")
                    | Text.Block("• BERT-Base-Uncased accuracy improved to 0.92");
    }
}
