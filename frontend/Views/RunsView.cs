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

    var openNewRunDialog = () =>
    {
      client.Toast("Opening Log Run Dialog...");
    };

    var header = new Card(
        Layout.Horizontal().Align(Align.Center)
            | Layout.Vertical()
                | Text.H2("Experiment Runs")
                | Text.P($"{filteredRuns.Count} runs matching filters")
                | searchTerm.ToTextInput().Placeholder("Search runs...")
            | Layout.Horizontal().Gap(2)
                | new Button("New Run", openNewRunDialog).Primary()
                | new DropDownMenu(
                    evt => selectedTag.Set(evt.Value?.ToString() ?? "All Tags"),
                    MenuItem.Default("All Tags").Tag("All Tags"),
                    MenuItem.Default("vision").Tag("vision"),
                    MenuItem.Default("nlp").Tag("nlp")
                )
                | new Button("All Owners").Icon(Icons.User)
    );

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
        | new Card(table);
  }
}
