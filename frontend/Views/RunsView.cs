namespace Frontend.Views;

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

    // Dialog State
    var isDialogOpen = UseState(false);
    var runName = UseState("");
    var runTags = UseState("");
    var runOwner = UseState("");
    var runHyperparams = UseState("{\n  \"learning_rate\": 0.001,\n  \"batch_size\": 32\n}");

    // Mock Data
    var runs = new[]
    {
            new { Name = "ResNet-50-v1", Tags = "vision, classification", Owner = "Alice", Stage = "Production", Accuracy = 0.94, CreatedAt = "2024-02-01" },
            new { Name = "BERT-Base-Uncased", Tags = "nlp, transformer", Owner = "Bob", Stage = "Staging", Accuracy = 0.89, CreatedAt = "2024-02-03" },
            new { Name = "YOLOv8-Nano", Tags = "vision, detection", Owner = "Charlie", Stage = "Production", Accuracy = 0.91, CreatedAt = "2024-02-04" },
            new { Name = "GPT-2-Large", Tags = "nlp, generation", Owner = "Alice", Stage = "Staging", Accuracy = 0.85, CreatedAt = "2024-02-04" }
        };

    var filteredRuns = runs.Where(r =>
        (string.IsNullOrEmpty(searchTerm.Value) || r.Name.Contains(searchTerm.Value, StringComparison.OrdinalIgnoreCase)) &&
        (selectedTag.Value == "All Tags" || r.Tags.Contains(selectedTag.Value))
    ).ToList();

    var labeledInput = (string label, object input) =>
        Layout.Vertical().Gap(1)
            | Text.Block(label).Bold()
            | input;

    var header = Layout.Horizontal().Align(Align.Center).Gap(4)
        | Text.H2("Experiment Runs")
        | Text.P($"{filteredRuns.Count} runs")
        | searchTerm.ToTextInput().Placeholder("Search runs...").Width(200)
        | new Spacer()
        | Layout.Horizontal().Gap(2)
            | new Button("New Run", () => isDialogOpen.Set(true)).Primary()
            | (new DropDownMenu(
                evt => selectedTag.Set(evt.Value?.ToString() ?? "All Tags"),
                new Button(selectedTag.Value).Icon(Icons.Tag))
                | MenuItem.Default("All Tags").Tag("All Tags")
                | MenuItem.Default("vision").Tag("vision")
                | MenuItem.Default("nlp").Tag("nlp"))
            | (new DropDownMenu(evt => client.Toast($"Selected Owner: {evt.Value}"),
                new Button("All Owners").Icon(Icons.User))
                | MenuItem.Default("All Owners").Tag("All Owners")
                | MenuItem.Default("Alice").Tag("Alice")
                | MenuItem.Default("Bob").Tag("Bob")
                | MenuItem.Default("Charlie").Tag("Charlie"));

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



    return Layout.Vertical().Gap(6)
        | header
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
