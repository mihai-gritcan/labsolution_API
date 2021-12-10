using LabSolution.Dtos;
using LabSolution.Infrastructure;
using LabSolution.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Text.Json;

namespace LabSolution.Services
{
    public interface IAppConfigService
    {
        Task<List<AppConfigDto>> GetAppConfigurations();
        Task<LabConfigAddresses> GetLabConfigAddresses();
        Task<LabConfigOpeningHours> GetLabConfigOpeningHours();
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

        public async Task<LabConfigAddresses> GetLabConfigAddresses()
        {
            var objType = typeof(LabConfigAddresses);
            var props = new List<PropertyInfo>(objType.GetProperties());

            var keysToRetrieve = props.Select(x => x.Name.ToUpper()).ToList();

            var configs = await _context.AppConfigs.Where(x => keysToRetrieve.Contains(x.Key.ToUpper()))
                                    .ToDictionaryAsync(x => x.Key.ToUpper(), x => x.Value);

            return new LabConfigAddresses
            {
                LabAddress = configs.ContainsKey(nameof(LabConfigAddresses.LabAddress).ToUpper()) ? configs[nameof(LabConfigAddresses.LabAddress).ToUpper()] : "",
                LabName = configs.ContainsKey(nameof(LabConfigAddresses.LabName).ToUpper()) ? configs[nameof(LabConfigAddresses.LabName).ToUpper()] : "",
                WebSiteAddress = configs.ContainsKey(nameof(LabConfigAddresses.WebSiteAddress).ToUpper()) ? configs[nameof(LabConfigAddresses.WebSiteAddress).ToUpper()] : "",
                PhoneNumber = configs.ContainsKey(nameof(LabConfigAddresses.PhoneNumber).ToUpper()) ? configs[nameof(LabConfigAddresses.PhoneNumber).ToUpper()] : "",
                TestEquipmentAnalyzer = configs.ContainsKey(nameof(LabConfigAddresses.TestEquipmentAnalyzer).ToUpper()) ? configs[nameof(LabConfigAddresses.TestEquipmentAnalyzer).ToUpper()] : ""
            };
        }


        public async Task<LabConfigOpeningHours> GetLabConfigOpeningHours()
        {
            const string defaultStartTime = "08:00";
            const string defaultEndDayTime = "18:00";
            const int defaultIntervalDurationMinutes = 10;
            const int defaultPersonsInInterval = 2;
            var defaultWorkingDays = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

            var objType = typeof(LabConfigOpeningHours);
            var props = new List<PropertyInfo>(objType.GetProperties());

            var keysToRetrieve = props.ConvertAll(x => x.Name.ToUpper());

            var configs = await _context.AppConfigs.Where(x => keysToRetrieve.Contains(x.Key.ToUpper()))
                                    .ToDictionaryAsync(x => x.Key.ToUpper(), x => x.Value);

            return new LabConfigOpeningHours
            {
                StartDayTime = configs.ContainsKey(nameof(LabConfigOpeningHours.StartDayTime).ToUpper()) ? configs[nameof(LabConfigOpeningHours.StartDayTime).ToUpper()] : defaultStartTime,
                EndDayTime = configs.ContainsKey(nameof(LabConfigOpeningHours.EndDayTime).ToUpper()) ? configs[nameof(LabConfigOpeningHours.EndDayTime).ToUpper()] : defaultEndDayTime,
                IntervalDurationMinutes = configs.ContainsKey(nameof(LabConfigOpeningHours.IntervalDurationMinutes).ToUpper())
                    ? int.Parse(configs[nameof(LabConfigOpeningHours.IntervalDurationMinutes).ToUpper()]) : defaultIntervalDurationMinutes,
                PersonsInInterval = configs.ContainsKey(nameof(LabConfigOpeningHours.PersonsInInterval).ToUpper())
                    ? int.Parse(configs[nameof(LabConfigOpeningHours.PersonsInInterval).ToUpper()]) : defaultPersonsInInterval,
                WorkingDays = configs.ContainsKey(nameof(LabConfigOpeningHours.WorkingDays).ToUpper())
                    ? JsonSerializer.Deserialize<List<string>>(configs[nameof(LabConfigOpeningHours.WorkingDays).ToUpper()]) : defaultWorkingDays
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

            _context.AppConfigs.AddRange(configsToAdd);
            _context.UpdateRange(configsToUpdate);

            await _context.SaveChangesAsync();

            return configsToAdd.Union(configsToUpdate).Select(x => new AppConfigDto { Id = x.Id, Key = x.Key, Value = x.Value });
        }
    }
}
