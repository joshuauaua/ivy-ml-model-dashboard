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
        | new Card()
            | Layout.Horizontal().Align(Align.Center).Gap(4)
                | new Badge("Live").Success()
                | Text.H2("Current Production Model: ResNet-50-v1")
        | Layout.Grid().Columns(3).Gap(6)
            | new Card()
                .Header("Requests / sec")
                | Layout.Vertical().Align(Align.Center)
                    | Text.H2("450")
                    | new Badge("+12% from last hour").Primary()
            | new Card()
                .Header("Error Rate")
                | Layout.Vertical().Align(Align.Center)
                    | Text.H2("0.04%")
                    | new Badge("Healthy").Success()
            | new Card()
                .Header("Average Latency (ms)")
                | Layout.Vertical().Align(Align.Center)
                    | Text.H2("12.5")
                    | new Badge("Stable").Info()
        | new Card()
            .Header("Live Requests / sec")
            | new AreaChart(data) // Trying AreaChart with data directly
                .Height(300);
  }
}
