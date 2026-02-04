using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IvyMLDashboard.Data;
using IvyMLDashboard.Models;

namespace IvyMLDashboard.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class DashboardController : ControllerBase
  {
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
      _context = context;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetStats()
    {
      var totalRuns = await _context.Runs.CountAsync();
      var activeDeployments = await _context.Deployments.CountAsync(d => d.Health == DeploymentHealth.Active);
      var lastRun = await _context.Runs.OrderByDescending(r => r.CreatedAt).FirstOrDefaultAsync();

      return new
      {
        TotalRuns = totalRuns,
        ActiveDeployments = activeDeployments,
        LastRun = lastRun?.Name ?? "N/A",
        AverageAccuracy = totalRuns > 0 ? await _context.Runs.AverageAsync(r => r.Accuracy) : 0
      };
    }
  }
}
