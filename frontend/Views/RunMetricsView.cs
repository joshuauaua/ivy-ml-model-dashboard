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

    // Fetch the specific run details to get real metrics
    var runQuery = UseQuery<Run, string>(
      $"run:{_runName}",
      async (key, ct) =>
      {
        using var httpClient = new System.Net.Http.HttpClient();
        httpClient.BaseAddress = new Uri("http://localhost:5153/");
        var response = await httpClient.GetAsync("api/runs", ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        var runs = System.Text.Json.JsonSerializer.Deserialize<List<Run>>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return runs?.FirstOrDefault(r => r.Name == _runName) ?? new Run { Name = _runName };
      }
    );

    var run = runQuery.Value ?? new Run { Name = _runName };

    var header = Layout.Horizontal().Align(Align.Center)
        | new Button("Back", _onBack).Secondary()
        | new Spacer()
        | Layout.Horizontal().Gap(2).Align(Align.Center)
            | Text.H2(_runName)
            | (run.Stage == 2 ? new Badge("Production").Success() : new Badge("Staging").Info())
        | new Spacer()
        | (run.Stage == 1 ? new Button("Promote to Prod").Primary().Icon(Icons.Zap) : null);

    // Dedicated Binary Classification Metrics Table as requested
    var performanceMetrics = new Card()
        .Header("Binary Classification Metrics (ML.NET)")
        | Layout.Vertical().Gap(4)
            | Text.P("Evaluation metrics calculated based on the official Microsoft ML.NET documentation.").Small().Italic()
            | new Table()
                | new TableRow()
                    | new TableCell(Text.Block("Metric").Bold())
                    | new TableCell(Text.Block("Value").Bold())
                    | new TableCell(Text.Block("Description").Bold())
                | new TableRow()
                    | new TableCell("Accuracy")
                    | new TableCell(Text.H3(run.Accuracy.ToString("P1")))
                    | new TableCell("The proportion of correct predictions among the total number of cases.")
                | new TableRow()
                    | new TableCell("AUC (Area Under ROC)")
                    | new TableCell(Text.H3(run.AreaUnderRocCurve.ToString("F3")))
                    | new TableCell("Indicates how well the model distinguishes between positive and negative classes.")
                | new TableRow()
                    | new TableCell("F1 Score")
                    | new TableCell(Text.H3(run.F1Score.ToString("F3")))
                    | new TableCell("The harmonic mean of Precision and Recall, balancing both metrics.")
                | new TableRow()
                    | new TableCell("Precision")
                    | new TableCell(Text.H3(run.Precision.ToString("P1")))
                    | new TableCell("The proportion of correct positive predictions.")
                | new TableRow()
                    | new TableCell("Recall")
                    | new TableCell(Text.H3(run.Recall.ToString("P1")))
                    | new TableCell("The proportion of actual positives that were correctly identified.")
                | new TableRow()
                    | new TableCell("Log-Loss")
                    | new TableCell(Text.H3(run.LogLoss.ToString("F4")))
                    | new TableCell("A measure of the divergence between predicted probabilities and actual labels.");

    var confusionMatrix = new Card()
        .Header("Mock Confusion Matrix")
        | Layout.Vertical().Align(Align.Center)
            | Text.P("Predicted").Small()
            | Layout.Horizontal().Gap(2)
                | Layout.Vertical().Align(Align.Center) | Text.P("Actual").Small()
                | Layout.Grid().Columns(2).Rows(2).Gap(2)
                    | (Layout.Vertical().Align(Align.Center) | new Card() | Text.H3("942") | Text.P("True Positive").Small())
                    | (Layout.Vertical().Align(Align.Center) | new Card() | Text.H3("58") | Text.P("False Negative").Small())
                    | (Layout.Vertical().Align(Align.Center) | new Card() | Text.H3("23") | Text.P("False Positive").Small())
                    | (Layout.Vertical().Align(Align.Center) | new Card() | Text.H3("877") | Text.P("True Negative").Small());

    return Layout.Vertical().Gap(6).Padding(8)
        | header
        | Layout.Grid().Columns(2).Gap(6)
            | performanceMetrics
            | Layout.Vertical().Gap(6)
                | confusionMatrix
                | new Card().Header("Hyperparameters") 
                    | Text.Block(run.Hyperparameters).Small().Italic();
  }
}
