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
    public class TrainingProgramService: ITrainingProgramService
    {
        private readonly HttpClient _client;
        private readonly ILogger<TrainingProgramService> _logger;

        public TrainingProgramService(IHttpClientFactory clientFactory, ILogger<TrainingProgramService> logger) {

            _client = clientFactory.CreateClient("trainingProgramService");
            _client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("TRAINING_PROGRAM_API_BASE_URL"));
            _client.DefaultRequestHeaders.Add("X-Api-Key", Environment.GetEnvironmentVariable("CONTENTS_API_KEY"));
            _client.Timeout = TimeSpan.FromSeconds(30);

            _logger = logger;
        }

        /// <summary> This method retrieves a boolean indicating wether a given training program id exists or not.
        /// The service is configured through the env variable named TRAINING_PROGRAM_API_BASE_URL
        /// The HttpClient used is configured in Startup.cs
        /// </summary>
        public async Task<bool> CheckIfTrainingProgramExists(string id)
        {
            try
            {
                TrainingProgramAPIRepresentation trainingProgramResponse = null;

                string url = $"{_client.BaseAddress}/{id}";
                HttpResponseMessage response = await _client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                trainingProgramResponse = await response.Content.ReadFromJsonAsync<TrainingProgramAPIRepresentation>();

                _logger.LogInformation(
                    $"TrainingProgramService API response (CheckIfTrainingProgramExists): {trainingProgramResponse}"
                );

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    $"TrainingProgramService (CheckIfTrainingProgramExists) - Unhandled Exception fetching id '{id}'"
                );

                return false;
            }
        }
    }
}
