namespace Frontend.Views;

public class Run
{
  public int Id { get; set; }
  public string Name { get; set; } = "";
  public string Tags { get; set; } = "";
  public string Owner { get; set; } = "";
  public string Scenario { get; set; } = "Classification";
  public int Stage { get; set; }
  public double Accuracy { get; set; }
  public double AreaUnderRocCurve { get; set; }
  public double F1Score { get; set; }
  public double Precision { get; set; }
  public double Recall { get; set; }
  public double LogLoss { get; set; }
  // Regression Metrics
  public double RSquared { get; set; }
  public double MeanAbsoluteError { get; set; }
  public double MeanSquaredError { get; set; }
  public double RootMeanSquaredError { get; set; }
  public string Hyperparameters { get; set; } = "{}";
  public string CreatedAt { get; set; } = "";
}

public class RunsView : ViewBase
{
  private readonly Action<string> _onViewMetrics;

  public RunsView(Action<string> onViewMetrics)
  {
    _onViewMetrics = onViewMetrics;
  }

  public override object? Build()
  {
    var client = UseService<IClientProvider>();

    var searchTerm = UseState("");
    var selectedTag = UseState("All Tags");
    var selectedOwner = UseState("All Owners");

    // Dialog State
    var isDialogOpen = UseState(false);
    var runName = UseState("");
    var runTags = UseState("");
    var runOwner = UseState("");
    var runScenario = UseState("Classification");
    var isTraining = UseState(true);
    var runHyperparams = UseState("{\n  \"train_time\": 60,\n  \"dataset\": \"yelp_labelled.txt\"\n}");

    // UseQuery for automatic updates
    var runsQuery = UseQuery<List<Run>, string>(
      "runs",
      async (key, ct) =>
      {
        using var httpClient = new System.Net.Http.HttpClient();
        httpClient.BaseAddress = new Uri("http://localhost:5153/");
        var response = await httpClient.GetAsync("api/runs", ct);
        response.EnsureSuccessStatusCode();
        var responseJson = await response.Content.ReadAsStringAsync(ct);
        return System.Text.Json.JsonSerializer.Deserialize<List<Run>>(responseJson, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Run>();
      },
      new QueryOptions { RefreshInterval = TimeSpan.FromSeconds(3) }
    );

    var currentRuns = runsQuery.Value ?? new List<Run>();

    var filteredRuns = currentRuns.Where(r =>
        (string.IsNullOrEmpty(searchTerm.Value) || r.Name.Contains(searchTerm.Value, StringComparison.OrdinalIgnoreCase)) &&
        (selectedTag.Value == "All Tags" || r.Tags.Contains(selectedTag.Value)) &&
        (selectedOwner.Value == "All Owners" || r.Owner == selectedOwner.Value)
    ).ToList();

    var labeledInput = (string label, object input) =>
        Layout.Vertical().Gap(1)
            | Text.Block(label).Bold()
            | input;

    var tagOptions = new[] { "All Tags", "vision", "nlp" }.ToOptions();
    var ownerOptions = new[] { "All Owners", "Alice", "Bob", "Charlie" }.ToOptions();

    var topHeader = Layout.Grid().Columns(2)
        | (Layout.Horizontal().Align(Align.Left) | Text.H1("Experiment Runs"))
        | (Layout.Horizontal().Align(Align.Right) | new Button("New Run", () => isDialogOpen.Set(true)).Primary());

    var middleHeader = Layout.Grid().Columns(2)
        | (Layout.Horizontal().Align(Align.Left) | Text.P($"{filteredRuns.Count} runs"))
        | (Layout.Horizontal().Align(Align.Right)
            | selectedTag.ToSelectInput(tagOptions).Width(150));

    var bottomHeader = Layout.Grid().Columns(2)
        | (Layout.Horizontal().Align(Align.Left) | searchTerm.ToTextInput().Placeholder("Search runs...").Width(250))
        | (Layout.Horizontal().Align(Align.Right)
            | selectedOwner.ToSelectInput(ownerOptions).Width(150));

    var table = new Table()
        | new TableRow()
            | new TableCell(Text.Block("Run Name").Bold())
            | new TableCell(Text.Block("Stage").Bold())
            | new TableCell(Text.Block("Scenario").Bold())
            | new TableCell(Text.Block("Accuracy").Bold())
            | new TableCell(Text.Block("AUC").Bold())
            | new TableCell(Text.Block("F1").Bold())
            | new TableCell(Text.Block("Prec/Rec").Bold())
            | new TableCell(Text.Block("LogLoss").Bold())
            | new TableCell(Text.Block("Actions").Bold());

    foreach (var run in filteredRuns)
    {
      string stageText = run.Stage switch
      {
        0 => "Training",
        1 => "Staging",
        2 => "Production",
        _ => "Unknown"
      };

      table |= new TableRow()
          | new TableCell(run.Name)
          | new TableCell(stageText == "Training" ? new Badge(stageText).Warning() : (stageText == "Production" ? new Badge(stageText).Success() : new Badge(stageText).Info()))
          | new TableCell(new Badge(run.Scenario).Info())
          | new TableCell(run.Accuracy > 0 ? run.Accuracy.ToString("P1") : "N/A")
          | new TableCell(run.AreaUnderRocCurve > 0 ? run.AreaUnderRocCurve.ToString("F3") : "N/A")
          | new TableCell(run.F1Score > 0 ? run.F1Score.ToString("F3") : "N/A")
          | new TableCell(run.Precision > 0 || run.Recall > 0 ? $"{run.Precision.ToString("P0")}/{run.Recall.ToString("P0")}" : "N/A")
          | new TableCell(run.LogLoss > 0 ? run.LogLoss.ToString("F4") : "N/A")
          | new TableCell(
              Layout.Horizontal().Gap(2)
              | new Button("View Charts", () => _onViewMetrics(run.Name)).Secondary()
              | (run.Stage == 1 ? new Button("Promote", async () =>
              {
                using var httpClient = new System.Net.Http.HttpClient();
                httpClient.BaseAddress = new Uri("http://localhost:5153/");
                var response = await httpClient.PostAsync($"api/runs/{run.Id}/promote", null);
                if (response.IsSuccessStatusCode)
                {
                  client.Toast($"Promoted {run.Name} to Production!");
                }
              }).Primary().Icon(Icons.Zap) : null)
            );
    }

    return Layout.Vertical().Gap(8).Padding(8)
        | (Layout.Vertical().Gap(0)
            | topHeader
            | middleHeader
            | bottomHeader)
        | new Card(table)
        | (isDialogOpen.Value ? new Dialog(
            _ => isDialogOpen.Set(false),
            new DialogHeader("Log a New Experiment Run"),
            new DialogBody(
                Layout.Vertical().Gap(4)
                | labeledInput("Run Name", runName.ToTextInput().Placeholder("e.g. ResNBA-101"))
                | labeledInput("Tags (comma-separated)", runTags.ToTextInput().Placeholder("vision, production"))
                | labeledInput("Owner", runOwner.ToTextInput().Placeholder("e.g. joshua"))
                | runScenario.ToSelectInput(new string[] { "Classification", "Image classification", "Regression", "Forecasting", "Recommendation" }.ToOptions())
                         .Variant(SelectInputs.Select)
                         .WithField()
                         .Label("Scenario")
                         .Width(Size.Full())
                | labeledInput("Train Model with ML.NET AutoML?",
                    Layout.Horizontal().Align(Align.Left).Gap(2)
                        | isTraining.ToBoolInput()
                        | Text.Block("Enable to trigger background training session").Italic())
                | labeledInput("Hyperparameters (JSON Format)", runHyperparams.ToTextInput())
                | Text.Block("Available datasets: yelp_labelled.txt, amazon_cells_labelled.txt, imdb_labelled.txt").Italic().Small()
            ),
            new DialogFooter(
                new Button("Cancel", () => isDialogOpen.Set(false)),
                new Button("Log Run", async () =>
                {
                  using var httpClient = new System.Net.Http.HttpClient();
                  httpClient.BaseAddress = new Uri("http://localhost:5153/");

                  if (isTraining.Value)
                  {
                    client.Toast($"Triggering ML.NET AutoML training for {runName.Value}...");

                    var trainRequest = new
                    {
                      Name = runName.Value,
                      Tags = runTags.Value,
                      Owner = runOwner.Value,
                      Scenario = runScenario.Value,
                      Hyperparameters = runHyperparams.Value
                    };

                    try
                    {
                      var json = System.Text.Json.JsonSerializer.Serialize(trainRequest);
                      var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
                      var response = await httpClient.PostAsync("api/runs/train", content);
                    }
                    catch (Exception ex)
                    {
                      client.Toast($"Error triggering training: {ex.Message}");
                    }
                  }
                  else
                  {
                    var staticRun = new
                    {
                      Name = runName.Value,
                      Tags = runTags.Value,
                      Owner = runOwner.Value,
                      Scenario = runScenario.Value,
                      Stage = 1, // Staging
                      Accuracy = 0.85,
                      RSquared = 0.82,
                      CreatedAt = DateTime.UtcNow
                    };

                    try
                    {
                      var json = System.Text.Json.JsonSerializer.Serialize(staticRun);
                      var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
                      var response = await httpClient.PostAsync("api/runs", content);
                    }
                    catch (Exception ex)
                    {
                      client.Toast($"Error logging run: {ex.Message}");
                    }
                  }

                  isDialogOpen.Set(false);
                  runName.Set("");
                  runTags.Set("");
                  runOwner.Set("");
                }).Primary()
            )
        ) : null);

  }
}
