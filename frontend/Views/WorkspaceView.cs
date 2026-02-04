namespace Frontend.Views;

public class WorkspaceView : ViewBase
{
  public override object? Build()
  {
    return Layout.Vertical().Gap(8).Padding(8)
        | Text.H1("Workspace Overview")

        | (Layout.Grid().Columns(3).Gap(6)
            | new Card(
                Layout.Horizontal().Align(Align.Left)
                    | new Icon(Icons.Activity).Size(Size.Units(20))
                    | Layout.Vertical().Align(Align.Center).Gap(1)
                        | Text.Block("Active Runs").Bold()
                        | Text.H2("12")
            )
            | new Card(
                Layout.Horizontal().Align(Align.Left)
                    | new Icon(Icons.Check).Size(Size.Units(20))
                    | Layout.Vertical().Align(Align.Center).Gap(1)
                        | Text.Block("Healthy Deployments").Bold()
                        | Text.H2("8")
            )
            | new Card(
                Layout.Horizontal().Align(Align.Left)
                    | new Icon(Icons.Server).Size(Size.Units(20))
                    | new Spacer()
                    | Layout.Vertical().Align(Align.Center).Gap(1)
                        | Text.Block("System Status").Bold()
                        | Text.H2("Online")
            ))

        | (Layout.Grid().Columns(3).Gap(6)
            | new Card(
                Layout.Horizontal().Align(Align.Left)
                    | new Icon(Icons.Activity).Size(Size.Units(20))
                    | Layout.Vertical().Align(Align.Center).Gap(1)
                        | Text.Block("Active Runs").Bold()
                        | Text.H2("12")
            )
            | new Card(
                Layout.Horizontal().Align(Align.Left)
                    | new Icon(Icons.Check).Size(Size.Units(20))
                    | Layout.Vertical().Align(Align.Center).Gap(1)
                        | Text.Block("Healthy Deployments").Bold()
                        | Text.H2("8")
            )
            | new Card(
                Layout.Horizontal().Align(Align.Left)
                    | new Icon(Icons.Server).Size(Size.Units(20))
                    | new Spacer()
                    | Layout.Vertical().Align(Align.Center).Gap(1)
                        | Text.Block("System Status").Bold()
                        | Text.H2("Online")
            ))




        | Text.H2("Recent Activity")
        | new Card(
            Layout.Vertical().Gap(2)
                | Text.Block("• ResNet-50-v1 deployed to Production")
                | Text.Block("• YOLOv8-Nano training completed")
                | Text.Block("• BERT-Base-Uncased accuracy improved to 0.92")
        ).Title("Activity Log");
  }
}
