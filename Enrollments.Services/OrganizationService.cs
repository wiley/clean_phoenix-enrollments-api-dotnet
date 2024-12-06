using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Enrollments.Domain;
using Enrollments.Domain.Enrollment;
using Enrollments.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Enrollments.Services
{
    public class OrganizationService: IOrganizationService
    {
        private readonly HttpClient _client;
        private readonly ILogger<OrganizationService> _logger;

        public OrganizationService(IHttpClientFactory clientFactory, ILogger<OrganizationService> logger)
        {
            _client = clientFactory.CreateClient("organizationService");
            _client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("COMPANY_API_BASE_URL"));
            _client.DefaultRequestHeaders.Add("x-api-key", Environment.GetEnvironmentVariable("COMPANY_API_KEY"));
            _client.Timeout = TimeSpan.FromSeconds(30);

            _logger = logger;
        }

        /// <summary> This method retrieves a organization from its service.
        /// The service is configured through the env variable named COMPANY_API_BASE_URL
        /// The HttpClient used is configured in Startup.cs
        /// </summary>
        public async Task<bool> CheckIfOrganizationExists(int id)
        {
            try
            {
                OrganizationAPIRepresentation organizationResponse = null;

                string url = $"{_client.BaseAddress}/{id}";
                HttpResponseMessage response = await _client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                organizationResponse = await response.Content.ReadFromJsonAsync<OrganizationAPIRepresentation>();

                _logger.LogInformation(
                    $"OrganizationService API response (CheckIfOrganizationExists): {organizationResponse}"
                );

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"OrganizationService (GetOrganization) - Unhandled Exception");
                return false;
            }
        }
    }
}
