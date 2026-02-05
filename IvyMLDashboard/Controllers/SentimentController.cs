using Microsoft.AspNetCore.Mvc;
using SentimentModel.ConsoleApp;

namespace IvyMLDashboard.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class SentimentController : ControllerBase
  {
    [HttpPost("predict")]
    public ActionResult<SentimentResult> Predict([FromBody] PredictRequest request)
    {
      if (string.IsNullOrEmpty(request.Text))
      {
        return BadRequest("Text is required");
      }

      var input = new SentimentModel.ConsoleApp.SentimentModel.ModelInput
      {
        Col0 = request.Text
      };

      var result = SentimentModel.ConsoleApp.SentimentModel.Predict(input);

      return Ok(new SentimentResult
      {
        Text = request.Text,
        Prediction = result.PredictedLabel == 1 ? "Positive" : "Negative",
        Score = result.Score[1] // ML.NET typically has [Negative, Positive] scores for binary classification
      });
    }

    public class PredictRequest
    {
      public string Text { get; set; } = string.Empty;
    }

    public class SentimentResult
    {
      public string Text { get; set; } = string.Empty;
      public string Prediction { get; set; } = string.Empty;
      public float Score { get; set; }
    }
  }
}
