using LabSolution.Clients;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static LabSolution.Clients.GovSyncClient;

namespace LabSolution.Services
{
	public interface IHttpClientServiceImplementation
	{
		Task<List<CompanyDto>> Execute1();
		Task<List<CompanyDto>> Execute2();
	}

	public class HttpClientFactoryService : IHttpClientServiceImplementation
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly GovSyncClient _govSyncClient;
		private readonly JsonSerializerOptions _options;

		public HttpClientFactoryService(IHttpClientFactory httpClientFactory, GovSyncClient govSyncClient)
		{
			_httpClientFactory = httpClientFactory;
			_govSyncClient = govSyncClient;

			_options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
		}

		public async Task<List<CompanyDto>> Execute1()
		{
			return await GetCompaniesWithHttpClientFactory();
		}

		public async Task<List<CompanyDto>> Execute2()
		{
			return await GetCompaniesWithTypedClient();
		}

		private async Task<List<CompanyDto>> GetCompaniesWithHttpClientFactory()
		{
			var httpClient = _httpClientFactory.CreateClient("GovSyncClient");
			
			using var response = await httpClient.GetAsync("companies", HttpCompletionOption.ResponseHeadersRead);
			response.EnsureSuccessStatusCode();

			var stream = await response.Content.ReadAsStreamAsync();
			return await JsonSerializer.DeserializeAsync<List<CompanyDto>>(stream, _options);
		}

		private async Task<List<CompanyDto>> GetCompaniesWithTypedClient() => await _govSyncClient.GetCompanies();
	}
}
