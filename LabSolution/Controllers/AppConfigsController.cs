using LabSolution.Dtos;
using LabSolution.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LabSolution.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppConfigsController : BaseApiController
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

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<AppConfigDto>>> GetConfigs()
        {
            return Ok(await _appConfigService.GetAppConfigurations());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConfig(int id)
        {
            await _appConfigService.DeleteConfig(id);

            return NoContent();
        }

        [AllowAnonymous]
        [HttpGet("opening-hours")]
        public async Task<ActionResult<List<OpeningHoursDto>>> GetOpeningHours()
        {
            return Ok(await _appConfigService.GetOpeningHours());
        }

        [AllowAnonymous]
        [HttpPost("opening-hours")]
        public async Task<IActionResult> SaveOpeningHours([FromBody] List<OpeningHoursDto> openingHours)
        {
            return Ok(await _appConfigService.SaveOpeningHours(openingHours));
        }

        [AllowAnonymous]
        [HttpDelete("opening-hours/{id}")]
        public async Task<IActionResult> DeleteOpeningHours(int id)
        {
            await _appConfigService.DeleteOpeningHours(id);

            return NoContent();
        }
    }
}

