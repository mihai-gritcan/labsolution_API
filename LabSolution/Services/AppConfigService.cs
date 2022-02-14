using LabSolution.Dtos;
using LabSolution.Infrastructure;
using LabSolution.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Text.Json;
using System;
using LabSolution.Utils;

namespace LabSolution.Services
{
    public interface IAppConfigService
    {
        Task<List<AppConfigDto>> GetAppConfigurations();
        Task<LabConfigAddresses> GetLabConfigAddresses();
        Task<LabConfigOpeningHours> GetLabConfigOpeningHours();
        Task DeleteConfig(int id);
        Task<IEnumerable<AppConfigDto>> SaveConfigs(List<AppConfigDto> appConfigs);

        Task<List<OpeningHoursDto>> GetOpeningHours();
        Task<IEnumerable<OpeningHoursDto>> SaveOpeningHours(List<OpeningHoursDto> openingHours);
        Task DeleteOpeningHours(int id);
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
                TestEquipmentAnalyzer = configs.ContainsKey(nameof(LabConfigAddresses.TestEquipmentAnalyzer).ToUpper()) ? configs[nameof(LabConfigAddresses.TestEquipmentAnalyzer).ToUpper()] : "",
                DownloadPDFUrl = configs.ContainsKey(nameof(LabConfigAddresses.DownloadPDFUrl).ToUpper()) ? configs[nameof(LabConfigAddresses.DownloadPDFUrl).ToUpper()] : "",
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

            await _context.SaveChangesAsync();

            return configsToAdd.Union(configsToUpdate).Select(x => new AppConfigDto { Id = x.Id, Key = x.Key, Value = x.Value });
        }

        public async Task<IEnumerable<OpeningHoursDto>> SaveOpeningHours(List<OpeningHoursDto> openingHours)
        {
            var defaultApplicableFrom = new DateTime(2022, 1, 1);
            var defaultApplicableTo = new DateTime(3000, 12, 31);

            var daysNames = openingHours.Select(x => x.DayOfWeek).ToHashSet();

            var existingRecords = await _context.OpeningHours.Where(x => daysNames.Contains(x.DayOfWeek)).ToListAsync();

            var openingHoursToAdd = new List<OpeningHours>();
            var openingHoursToUpdate = new List<OpeningHours>();

            foreach (var item in openingHours)
            {
                var match = existingRecords.Find(x => x.DayOfWeek.Equals(item.DayOfWeek, StringComparison.InvariantCultureIgnoreCase));

                if (match is null) {
                    openingHoursToAdd.Add(new OpeningHours
                    {
                        DayOfWeek = item.DayOfWeek,
                        OpenTime = item.OpenTime,
                        CloseTime = item.CloseTime,
                        ApplicableFrom = item.ApplicableFrom ?? defaultApplicableFrom,
                        ApplicableTo = item.ApplicableTo ?? defaultApplicableTo
                    });
                }
                else
                {
                    match.OpenTime = item.OpenTime;
                    match.CloseTime = item.CloseTime;
                    openingHoursToUpdate.Add(match);
                }
            }

            _context.OpeningHours.AddRange(openingHoursToAdd);

            await _context.SaveChangesAsync();

            return openingHoursToAdd.Union(openingHoursToUpdate).Select(x => new OpeningHoursDto
            {
                Id = x.Id,
                DayOfWeek = x.DayOfWeek,
                OpenTime = x.OpenTime,
                CloseTime = x.CloseTime,
                ApplicableFrom = x.ApplicableFrom,
                ApplicableTo = x.ApplicableTo
            });
        }

        public async Task DeleteOpeningHours(int id)
        {
            var entity = await _context.OpeningHours.FindAsync(id);

            if (entity == null)
                throw new ResourceNotFoundException();

            _context.OpeningHours.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public Task<List<OpeningHoursDto>> GetOpeningHours()
        {
            return _context.OpeningHours.Select(x => new OpeningHoursDto
            {
                Id = x.Id,
                DayOfWeek = x.DayOfWeek,
                OpenTime = x.OpenTime,
                CloseTime = x.CloseTime,
                ApplicableFrom = x.ApplicableFrom,
                ApplicableTo = x.ApplicableTo
            }).ToListAsync();
        }
    }
}
