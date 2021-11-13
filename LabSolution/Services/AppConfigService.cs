using LabSolution.Dtos;
using LabSolution.Infrastructure;
using LabSolution.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace LabSolution.Services
{
    public interface IAppConfigService
    {
        Task<List<AppConfigDto>> GetAppConfigurations();
        Task<LabConfigOptions> GetLabConfigOptions();
        Task DeleteConfig(int id);
        Task<IEnumerable<AppConfigDto>> SaveConfigs(List<AppConfigDto> appConfigs);
    }

    public class AppConfigService : IAppConfigService
    {
        private readonly LabSolutionContext _context;

        public AppConfigService(LabSolutionContext context)
        {
            _context = context;
        }

        public async Task DeleteConfig(int id)
        {
            var config = await _context.AppConfigs.FindAsync(id);

            if (config == null)
                throw new ResourceNotFoundException();

            _context.AppConfigs.Remove(config);
            await _context.SaveChangesAsync();
        }

        public Task<List<AppConfigDto>> GetAppConfigurations()
        {
            return _context.AppConfigs.Select(x => new AppConfigDto { Id = x.Id, Key = x.Key, Value = x.Value }).ToListAsync();
        }

        public async Task<LabConfigOptions> GetLabConfigOptions()
        {
            var objType = typeof(LabConfigOptions);
            var props = new List<PropertyInfo>(objType.GetProperties());

            var keysToRetrieve = props.Select(x => x.Name.ToUpperInvariant()).ToList();

            var configs = await _context.AppConfigs.Where(x => keysToRetrieve.Contains(x.Key.ToUpperInvariant()))
                                    .ToDictionaryAsync(x => x.Key, x => x.Value);

            return new LabConfigOptions
            {
                LabAddress = configs[nameof(LabConfigOptions.LabAddress)],
                LabName = configs[nameof(LabConfigOptions.LabName)],
                WebSiteName = configs[nameof(LabConfigOptions.WebSiteName)],
                PhoneNumber = configs[nameof(LabConfigOptions.PhoneNumber)]
            };
        }

        public async Task<IEnumerable<AppConfigDto>> SaveConfigs(List<AppConfigDto> appConfigs)
        {
            var configsToAdd = new List<AppConfig>();
            var configsToUpdate = new List<AppConfig>();

            var allConfigs = await _context.AppConfigs.Select(x => x).ToListAsync();
            foreach (var item in appConfigs)
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

            return configsToAdd.Union(configsToUpdate).Select(x => new AppConfigDto { Id = x.Id, Key = x.Key, Value = x.Value });
        }
    }
}
