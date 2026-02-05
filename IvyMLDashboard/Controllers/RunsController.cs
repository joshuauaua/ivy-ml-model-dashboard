using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IvyMLDashboard.Data;
using IvyMLDashboard.Models;

namespace IvyMLDashboard.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class RunsController : ControllerBase
  {
    private readonly AppDbContext _context;
    private readonly IvyMLDashboard.Services.ITrainingService _trainingService;

    public RunsController(AppDbContext context, IvyMLDashboard.Services.ITrainingService trainingService)
    {
      _context = context;
      _trainingService = trainingService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Run>>> GetRuns()
    {
      return await _context.Runs.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Run>> GetRun(int id)
    {
      var run = await _context.Runs.FindAsync(id);
      if (run == null) return NotFound();
      return run;
    }

    [HttpPost]
    public async Task<ActionResult<Run>> CreateRun(Run run)
    {
      _context.Runs.Add(run);
      await _context.SaveChangesAsync();
      return CreatedAtAction(nameof(GetRun), new { id = run.Id }, run);
    }

    [HttpPost("train")]
    public async Task<ActionResult<Run>> TrainRun([FromBody] TrainRequest request)
    {
      var run = new Run
      {
        Name = request.Name,
        Tags = request.Tags,
        Owner = request.Owner,
        Scenario = request.Scenario,
        Hyperparameters = request.Hyperparameters,
        Stage = RunStage.Training,
        Accuracy = 0.0,
        CreatedAt = DateTime.UtcNow
      };

      _context.Runs.Add(run);
      await _context.SaveChangesAsync();

      int trainTime = 60;
      try
      {
        var hyperparams = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(request.Hyperparameters);
        if (hyperparams != null && hyperparams.TryGetValue("train_time", out var val))
        {
          trainTime = int.Parse(val.ToString()!);
        }
      }
      catch { }

      await _trainingService.StartTrainingAsync(run.Id, trainTime);

      return CreatedAtAction(nameof(GetRun), new { id = run.Id }, run);
    }

    [HttpPost("{id}/promote")]
    public async Task<ActionResult<Run>> PromoteRun(int id)
    {
      var run = await _context.Runs.FindAsync(id);
      if (run == null) return NotFound();

      try
      {
        // mlnet saves models in a folder named after the --name argument
        string sourcePath = Path.Combine("/Users/joshuang/Desktop/myMLApp", $"SentimentModel_Run{id}", $"SentimentModel_Run{id}.mlnet");
        string destPath = Path.Combine("/Users/joshuang/Desktop/myMLApp", "SentimentModel", "SentimentModel.mlnet");

        if (System.IO.File.Exists(sourcePath))
        {
          System.IO.File.Copy(sourcePath, destPath, true);
        }
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Failed to copy model file: {ex.Message}");
      }

      // Enforce ONLY ONE production deployment: demote ALL existing production runs
      var existingProdRuns = await _context.Runs.Where(r => r.Stage == RunStage.Production).ToListAsync();
      foreach (var prodRun in existingProdRuns)
      {
        if (prodRun.Id != id)
        {
          prodRun.Stage = RunStage.Staging;
        }
      }

      run.Stage = RunStage.Production;
      await _context.SaveChangesAsync();

      return Ok(run);
    }

    [HttpPost("{id}/rollback")]
    public async Task<ActionResult<Run>> RollbackRun(int id)
    {
      var run = await _context.Runs.FindAsync(id);
      if (run == null) return NotFound();

      run.Stage = RunStage.Staging;
      await _context.SaveChangesAsync();

      return Ok(run);
    }

    public class TrainRequest
    {
      public string Name { get; set; } = string.Empty;
      public string Tags { get; set; } = string.Empty;
      public string Owner { get; set; } = string.Empty;
      public string Scenario { get; set; } = "Classification";
      public string Hyperparameters { get; set; } = "{}";
    }
  }
}
