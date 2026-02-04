namespace Frontend.Views;

public class LiveView : ViewBase
{
  public override object? Build()
  {
    var data = new[] {
            new { X = "10:00", Y = 400 },
            new { X = "10:05", Y = 420 },
            new { X = "10:10", Y = 380 },
            new { X = "10:15", Y = 450 },
            new { X = "10:20", Y = 440 },
            new { X = "10:25", Y = 460 },
            new { X = "10:30", Y = 450 }
        };

    return Layout.Vertical().Gap(8).Padding(8)
        | Text.H1("Live Production Stats")
        | (Layout.Grid().Columns(2).Gap(6)
            | new Card(
                Layout.Horizontal().Align(Align.Left)
                    | new Icon(Icons.Activity).Size(Size.Units(20))
                    | Layout.Vertical().Align(Align.Center).Gap(1)
                        | Text.Block("Requests / sec").Bold()
                        | Text.H2("450")
            )
            | new Card(
                Layout.Horizontal().Align(Align.Left)
                    | new Icon(Icons.Check).Size(Size.Units(20))
                    | Layout.Vertical().Align(Align.Center).Gap(1)
                        | Text.Block("Error Rate").Bold()
                        | Text.H2("0.04%")
            ))

        | (Layout.Grid().Columns(2).Gap(6)
            | new Card(
                Layout.Horizontal().Align(Align.Left)
                    | new Icon(Icons.Activity).Size(Size.Units(20))
                    | Layout.Vertical().Align(Align.Center).Gap(1)
                        | Text.Block("Requests / sec").Bold()
                        | Text.H2("450")
            )
            | new Card(
                Layout.Horizontal().Align(Align.Left)
                    | new Icon(Icons.Check).Size(Size.Units(20))
                    | Layout.Vertical().Align(Align.Center).Gap(1)
                        | Text.Block("Error Rate").Bold()
                        | Text.H2("0.04%")
            ))

            | (Layout.Grid().Columns(2).Gap(6)
            | new Card(
                Layout.Horizontal().Align(Align.Left)
                    | new Icon(Icons.Activity).Size(Size.Units(20))
                    | Layout.Vertical().Align(Align.Center).Gap(1)
                        | Text.Block("Requests / sec").Bold()
                        | Text.H2("450")
            )
            | new Card(
                Layout.Horizontal().Align(Align.Left)
                    | new Icon(Icons.Check).Size(Size.Units(20))
                    | Layout.Vertical().Align(Align.Center).Gap(1)
                        | Text.Block("Error Rate").Bold()
                        | Text.H2("0.04%")
            ))


        | new Card()
            .Header("Live Requests / sec")
            | new AreaChart(data) // Trying AreaChart with data directly
                .Height(300);
  }
}
