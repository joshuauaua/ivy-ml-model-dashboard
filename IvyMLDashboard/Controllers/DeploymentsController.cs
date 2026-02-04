using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IvyMLDashboard.Data;
using IvyMLDashboard.Models;

namespace IvyMLDashboard.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class DeploymentsController : ControllerBase
  {
    private readonly AppDbContext _context;

    public DeploymentsController(AppDbContext context)
    {
      _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Deployment>>> GetDeployments()
    {
      return await _context.Deployments.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Deployment>> GetDeployment(int id)
    {
      var deployment = await _context.Deployments.FindAsync(id);
      if (deployment == null) return NotFound();
      return deployment;
    }

    [HttpPost]
    public async Task<ActionResult<Deployment>> CreateDeployment(Deployment deployment)
    {
      _context.Deployments.Add(deployment);
      await _context.SaveChangesAsync();
      return CreatedAtAction(nameof(GetDeployment), new { id = deployment.Id }, deployment);
    }
  }
}
