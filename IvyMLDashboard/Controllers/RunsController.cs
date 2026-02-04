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

    public RunsController(AppDbContext context)
    {
      _context = context;
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
  }
}
