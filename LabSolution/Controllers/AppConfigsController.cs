using LabSolution.Dtos;
using LabSolution.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LabSolution.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AppConfigsController : ControllerBase
    {
        private readonly LabSolutionContext _context;

        public AppConfigsController(LabSolutionContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> SaveConfigs([FromBody] List<AppConfigDto> configs)
        {
            var configsToAdd = new List<AppConfig>();
            var configsToUpdate = new List<AppConfig>();

            var allConfigs = await _context.AppConfigs.Select(x => x).ToArrayAsync();
            foreach (var item in configs)
            {
                var match = allConfigs.FirstOrDefault(x => x.Id == item.Id && x.Key == item.Key);
                if (match is not null)
                {
                    match.Value = item.Value;
                    configsToUpdate.Add(match);
                }
                else
                {
                    configsToAdd.Add(new AppConfig { Id = item.Id, Key = item.Key, Value = item.Value });
                }
            }

            await _context.AppConfigs.AddRangeAsync(configsToAdd);
            _context.UpdateRange(configsToUpdate);

            await _context.SaveChangesAsync();

            return Ok(configsToAdd.Union(configsToUpdate).Select(x => new AppConfigDto { Id = x.Id, Key = x.Key, Value = x.Value }));
        }

        [HttpGet]
        public async Task<IActionResult> GetConfigs()
        {
            return Ok(await _context.AppConfigs.Select(x => new AppConfigDto { Id = x.Id, Key = x.Key, Value = x.Value }).ToListAsync());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConfig(int id)
        {
            var config = await _context.AppConfigs.FindAsync(id);
            if (config == null)
                return NotFound();

            _context.AppConfigs.Remove(config);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

