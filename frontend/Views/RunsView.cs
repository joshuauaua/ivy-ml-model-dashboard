namespace Frontend.Views;

public class Run
{
  public string Name { get; set; } = "";
  public string Tags { get; set; } = "";
  public string Owner { get; set; } = "";
  public string Stage { get; set; } = "Staging";
  public double Accuracy { get; set; }
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
    var runHyperparams = UseState("{\n  \"learning_rate\": 0.001,\n  \"batch_size\": 32\n}");

    // Stateful Runs Data
    var runs = UseState(new List<Run>
    {
            new Run { Name = "ResNet-50-v1", Tags = "vision, classification", Owner = "Alice", Stage = "Production", Accuracy = 0.94, CreatedAt = "2024-02-01" },
            new Run { Name = "BERT-Base-Uncased", Tags = "nlp, transformer", Owner = "Bob", Stage = "Staging", Accuracy = 0.89, CreatedAt = "2024-02-03" },
            new Run { Name = "YOLOv8-Nano", Tags = "vision, detection", Owner = "Charlie", Stage = "Production", Accuracy = 0.91, CreatedAt = "2024-02-04" },
            new Run { Name = "GPT-2-Large", Tags = "nlp, generation", Owner = "Alice", Stage = "Staging", Accuracy = 0.85, CreatedAt = "2024-02-04" }
        });

    var filteredRuns = runs.Value.Where(r =>
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
            | new TableCell(Text.Block("Tags").Bold())
            | new TableCell(Text.Block("Owner").Bold())
            | new TableCell(Text.Block("Stage").Bold())
            | new TableCell(Text.Block("Accuracy").Bold())
            | new TableCell(Text.Block("Created At").Bold())
            | new TableCell(Text.Block("Actions").Bold());

    foreach (var run in filteredRuns)
    {
      table |= new TableRow()
          | new TableCell(run.Name)
          | new TableCell(new Badge(run.Tags).Info())
          | new TableCell(run.Owner)
          | new TableCell(new Badge(run.Stage).Success())
          | new TableCell(run.Accuracy.ToString("P0"))
          | new TableCell(run.CreatedAt)
          | new TableCell(new Button("View Charts", () => _onViewMetrics(run.Name)).Secondary());
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
                | labeledInput("Hyperparameters (JSON Format)", runHyperparams.ToTextInput().Placeholder("e.g. {\"lr\": 0.01}"))
            ),
            new DialogFooter(
                new Button("Cancel", () => isDialogOpen.Set(false)),
                new Button("Log Run", () =>
                {
                  var newRun = new Run
                  {
                    Name = runName.Value,
                    Tags = runTags.Value,
                    Owner = runOwner.Value,
                    Stage = "Staging",
                    Accuracy = 0.0,
                    CreatedAt = "2024-02-05" // Simplification for now
                  };
                  runs.Set([.. runs.Value, newRun]);
                  client.Toast($"Logged run: {runName.Value}");
                  isDialogOpen.Set(false);
                  runName.Set("");
                  runTags.Set("");
                  runOwner.Set("");
                }).Primary()
            )
        ) : null);

  }
}
