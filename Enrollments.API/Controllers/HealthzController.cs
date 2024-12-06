using Enrollments.API.Responses;
using Enrollments.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Enrollments.API.Controllers
{
    [Route("v{version:apiVersion}/enrollments/healthz")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ApiVersion("4.0")]
    [ApiController]
    public class HealthzController : ControllerBase
    {
        private readonly ILogger<HealthzController> _logger;
        private readonly IMongoHealthCheckService _mongoHealth;

        public HealthzController(ILogger<HealthzController> logger, IMongoHealthCheckService mongoHealth)
        {
            this._logger = logger;
            this._mongoHealth = mongoHealth;
        }

        [HttpGet]
        [ProducesResponseType(typeof(StatusResponse), 200)]
        [ProducesResponseType(typeof(StatusResponse), 503)]
        public ActionResult<StatusResponse> Health()
        {
            var isAlive = _mongoHealth.IsAlive();
            if (isAlive)
            {
                _logger.LogInformation("Heath Check - Success");
            }
            else
            {
                _logger.LogInformation("Heath Check - Unable to connect to mongo DB");
                return StatusCode(503, new StatusResponse { Status = "Unable to connect to mongo DB" });
            }
            return Ok(new StatusResponse { Status = "OK" });
        }

        [HttpGet]
        [ProducesResponseType(typeof(HealthDependenciesResponse), 200)]
        [ProducesResponseType(typeof(HealthDependenciesResponse), 503)]
        [Route("dependencies")]
        public ActionResult<HealthDependenciesResponse> HealthDependencies()
        {
            var health = new HealthDependenciesResponse();
            health.Mongo = (_mongoHealth.IsAlive() ? "OK" : "Unavailable");
            if (health.Mongo == "OK")
            {
                return Ok(health);
            }
            else
            {
                return StatusCode(503, health);
            }
        }
    }
}
