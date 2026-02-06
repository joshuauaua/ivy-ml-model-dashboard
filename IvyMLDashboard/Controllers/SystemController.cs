using Microsoft.AspNetCore.Mvc;
using IvyMLDashboard.Data;
using IvyMLDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace IvyMLDashboard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SystemController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SystemController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("reset")]
        public async Task<IActionResult> ResetDatabase()
        {
            // 1. Delete all Deployments
            _context.Deployments.RemoveRange(_context.Deployments);
            
            // 2. Delete all Runs
            _context.Runs.RemoveRange(_context.Runs);
            
            await _context.SaveChangesAsync();

            // 3. Add single Example Run
            var exampleRun = new Run
            {
                Name = "Example-Run-01",
                Owner = "System",
                Tags = "example",
                Hyperparameters = "{\"train_time\": 10}",
                Stage = RunStage.Staging, // 1
                Scenario = "Classification",
                Accuracy = 0.80,
                CreatedAt = DateTime.UtcNow,
                
                // Set default values for other required fields if necessary
                AreaUnderRocCurve = 0,
                F1Score = 0,
                Precision = 0,
                Recall = 0,
                LogLoss = 0,
                RSquared = 0,
                MeanAbsoluteError = 0,
                MeanSquaredError = 0,
                RootMeanSquaredError = 0
            };

            _context.Runs.Add(exampleRun);
            await _context.SaveChangesAsync();

            return Ok(new { message = "System reset successfully", runId = exampleRun.Id });
        }
    }
}
