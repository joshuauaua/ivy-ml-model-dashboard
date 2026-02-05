namespace Frontend.Views;

public class LiveView : ViewBase
{
  public override object? Build()
  {
    var metricCard = (string label, string value, string trend, bool isWarning = false) =>
        new Card(
            Layout.Vertical().Gap(2).Padding(4)
                | Text.Block(label).Small().Muted()
                | Text.H2(value)
                | (isWarning ? new Badge(trend).Warning().Small() : new Badge(trend).Success().Small())
        );

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

        | new Card().Header("Infrastructure Health")
            | (Layout.Grid().Columns(3).Gap(4).Padding(6)
                | metricCard("Requests / sec", "450", "↑ 12%")
                | metricCard("Latency", "24ms", "Normal")
                | metricCard("Error Rate", "0.04%", "Stable"))

        | new Card().Header("Live Traffic Trend")
            | (Layout.Vertical().Padding(4)
                | new AreaChart(data).Height(300))

        | (Layout.Grid().Columns(2).Gap(6)
            | new Card().Header("Hardware Utilization")
                | (Layout.Grid().Columns(2).Gap(4).Padding(6)
                    | metricCard("CPU Usage", "42%", "Normal")
                    | metricCard("Memory", "1.2GB", "Stable"))

            | new Card().Header("Throughput")
                | (Layout.Grid().Columns(2).Gap(4).Padding(6)
                    | metricCard("Inbound", "8.2 MB/s", "Active")
                    | metricCard("Outbound", "12.4 MB/s", "Active")));
  }
}
