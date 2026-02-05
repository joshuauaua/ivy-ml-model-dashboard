namespace Frontend.Views;

public class DemoView : ViewBase
{
  public override object? Build()
  {
    var client = UseService<IClientProvider>();
    var reviewText = UseState("");
    var predictionResult = UseState("---");
    var confidenceScore = UseState("---");
    var isPositive = UseState(true);
    var isAnalyzing = UseState(false);

    return Layout.Vertical().Gap(8).Padding(8)
        | Text.H1("ML Model Demo")
        | Text.P("Experience the Sentiment Analysis model in real-time. This model was trained on Yelp reviews and categorizes text as Positive or Negative.")

        | (Layout.Horizontal().Gap(6).Align(Align.Stretch)
            | new Card(
                Layout.Vertical().Gap(6).Padding(4)
                    | Text.Block("Enter your review below:").Bold()
                    | (reviewText.ToTextInput().Placeholder("e.g. The food was delicious!")
                        .Width(350))

                    | new Button("Analyze Sentiment", async () =>
                    {
                      if (string.IsNullOrWhiteSpace(reviewText.Value))
                      {
                        client.Toast("Please enter some text to analyze.");
                        return;
                      }

                      isAnalyzing.Set(true);
                      try
                      {
                        using var httpClient = new System.Net.Http.HttpClient();
                        httpClient.BaseAddress = new Uri("http://localhost:5153/");

                        var request = new { Text = reviewText.Value };
                        var json = System.Text.Json.JsonSerializer.Serialize(request);
                        var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");

                        var response = await httpClient.PostAsync("api/sentiment/predict", content);
                        if (response.IsSuccessStatusCode)
                        {
                          var responseJson = await response.Content.ReadAsStringAsync();
                          using var doc = System.Text.Json.JsonDocument.Parse(responseJson);
                          var root = doc.RootElement;

                          string prediction = root.GetProperty("prediction").GetString() ?? "---";
                          float score = root.GetProperty("score").GetSingle();

                          predictionResult.Set(prediction);
                          confidenceScore.Set((score * 100).ToString("F1") + "%");
                          isPositive.Set(prediction == "Positive");
                          client.Toast("Analysis complete!");
                        }
                        else
                        {
                          client.Toast("Error calling sentiment API");
                        }
                      }
                      catch (Exception ex)
                      {
                        client.Toast($"Error: {ex.Message}");
                      }
                      finally
                      {
                        isAnalyzing.Set(false);
                      }
                    }).Primary()

                    | (isAnalyzing.Value ? Text.Block("Analyzing...").Italic() : (predictionResult.Value == "---" ? new Spacer().Height(20) : null))
            ).Width(400)

            | (predictionResult.Value != "---" ? new Card(
                Layout.Vertical().Gap(4).Padding(10).Align(Align.Center)
                    | Text.H2("Results")
                    | new Table()
                        | (new TableRow()
                            | new TableCell(Text.Block("Predicted Sentiment").Bold())
                            | new TableCell(Text.Block("Confidence Score").Bold()))
                        | (new TableRow()
                            | new TableCell(isPositive.Value ? new Badge("Positive").Success() : new Badge("Negative").Destructive())
                            | new TableCell(Text.H3(confidenceScore.Value)))
            ).Width(400) : null)
        );
  }
}
