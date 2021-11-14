using LabSolution.Dtos;
using LabSolution.Models;
using LabSolution.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LabSolution.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppConfigsController : ControllerBase
    {
        private readonly IAppConfigService _appConfigService;

        public AppConfigsController(IAppConfigService appConfigService)
        {
            _appConfigService = appConfigService;
        }

        [HttpPost]
        public async Task<IActionResult> SaveConfigs([FromBody] List<AppConfigDto> configs)
        {
            return Ok(await _appConfigService.SaveConfigs(configs));
        }

        [HttpGet]
        public async Task<IActionResult> GetConfigs()
        {
            return Ok(await _appConfigService.GetAppConfigurations());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConfig(int id)
        {
            await _appConfigService.DeleteConfig(id);

            return NoContent();
        }
    }
}

