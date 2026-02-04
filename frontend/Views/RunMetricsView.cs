namespace Frontend.Views;

public class RunMetricsView : ViewBase
{
  private readonly string _runName;
  private readonly Action _onBack;

  public RunMetricsView(string runName, Action onBack)
  {
    _runName = runName;
    _onBack = onBack;
  }

  public override object? Build()
  {
    var client = UseService<IClientProvider>();
    var importanceMetric = UseState("acc");
    var searchTerm = UseState("");

    var header = Layout.Horizontal().Align(Align.Center)
        | new Button("Back", _onBack).Secondary()
        | new Spacer()
        | Layout.Horizontal().Gap(2).Align(Align.Center)
            | Text.H2(_runName)
            | new Badge("Production").Success()
        | new Spacer()
        | new Button("Promote to Prod").Primary().Icon(Icons.Zap);

    var parameterImportance = new Card()
        .Header("Parameter Importance")
        | Layout.Vertical().Gap(4)
            | Layout.Horizontal().Gap(4).Align(Align.Center)
                | Text.P("Importance with respect to:").Small()
                | new DropDownMenu(
                    evt => importanceMetric.Set(evt.Value?.ToString() ?? "acc"),
                    MenuItem.Default("Accuracy").Tag("acc"),
                    MenuItem.Default("Precision").Tag("precision")
                )
                | searchTerm.ToTextInput().Placeholder("Search config parameters...")
            | new Table()
                | new TableRow()
                    | new TableCell(Text.Block("Config Parameter").Bold())
                    | new TableCell(Text.Block("Importance").Bold())
                    | new TableCell(Text.Block("Correlation").Bold())
                | new TableRow()
                    | new TableCell("learning_rate")
                    | new TableCell(Layout.Horizontal().Align(Align.Center) | new Card().Height(10).Width(100) | Text.P(" 0.85").Small())
                    | new TableCell(Layout.Horizontal().Align(Align.Center) | new Card().Height(10).Width(80) | Text.P(" 0.62").Small())
                | new TableRow()
                    | new TableCell("batch_size")
                    | new TableCell(Layout.Horizontal().Align(Align.Center) | new Card().Height(10).Width(40) | Text.P(" 0.34").Small())
                    | new TableCell(Layout.Horizontal().Align(Align.Center) | new Card().Height(10).Width(20) | Text.P(" -0.15").Small());

    var confusionMatrix = new Card()
        .Header("Confusion Matrix")
        | Layout.Vertical().Align(Align.Center)
            | Text.P("Predicted").Small()
            | Layout.Horizontal().Gap(2)
                | Layout.Vertical().Align(Align.Center) | Text.P("Actual").Small()
                | Layout.Grid().Columns(2).Rows(2).Gap(2)
                    | (Layout.Vertical().Align(Align.Center) | new Card() | Text.H3("942") | Text.P("PP").Small())
                    | (Layout.Vertical().Align(Align.Center) | new Card() | Text.H3("58") | Text.P("PN").Small())
                    | (Layout.Vertical().Align(Align.Center) | new Card() | Text.H3("23") | Text.P("NP").Small())
                    | (Layout.Vertical().Align(Align.Center) | new Card() | Text.H3("877") | Text.P("NN").Small());

    var data = new[] {
                    new { X = 0.5, Y = 0.4 },
                    new { X = 0.6, Y = 0.55 },
                    new { X = 0.7, Y = 0.68 },
                    new { X = 0.8, Y = 0.75 },
                    new { X = 0.85, Y = 0.82 },
                    new { X = 0.9, Y = 0.88 },
                    new { X = 0.94, Y = 0.91 }
                };

    var accVsPrecision = new Card()
        .Header("Accuracy vs Precision")
        | new AreaChart(data).Height(300);

    return Layout.Vertical().Gap(6)
        | header
        | Layout.Grid().Columns(2).Gap(6)
            | parameterImportance
            | Layout.Vertical().Gap(6)
                | confusionMatrix
                | accVsPrecision;
  }
}
