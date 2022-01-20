using LabSolution.Infrastructure;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace LabSolution.GovSync
{
    public class GovSyncClient
    {
		private readonly HttpClient _client;
		private readonly JsonSerializerOptions _deserializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
		private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

		public GovSyncClient(HttpClient client, IOptionsMonitor<GovSyncConfiguration> govSyncConfig)
		{
			var config = govSyncConfig.CurrentValue;

			_client = client;
			_client.BaseAddress = new Uri(config.ApiUrl);
			_client.Timeout = new TimeSpan(0, 0, 30);
			_client.DefaultRequestHeaders.Clear();
		}

		public async Task<SyncResultDto> SendTestResults(List<TestPushModel> testsToSync)
		{
			var syncResult = new SyncResultDto();

			foreach (var item in testsToSync)
			{
				try
				{
					await SendTest(item);
					syncResult.SynchedItems.Add(item);
				}
				catch (HttpRequestException ex)
				{
					syncResult.UnsynchedItems.Add(item, ex.Message);
				}
			}
			return syncResult;
		}

		private async Task SendTest(TestPushModel testToSync)
		{
            string jsonContent = JsonSerializer.Serialize(testToSync, options: _serializerOptions);
            var content = new StringContent(jsonContent, Encoding.UTF8, Application.Json);

			using var response = await _client.PostAsync("", content);
			response.EnsureSuccessStatusCode();
		}
	}
}
