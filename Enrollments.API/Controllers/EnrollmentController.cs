using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System.Collections.Generic;

using Enrollments.Services;
using Enrollments.Services.Interfaces;
using Enrollments.API.Responses;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Enrollments.API.Requests.Enrollment;
using System;
using Enrollments.Domain.Pagination;
using Enrollments.Domain.Enrollment;
using Enrollments.API.Helpers;
using Enrollments.Domain;
using WLS.Log.LoggerTransactionPattern;
using Microsoft.AspNetCore.Authorization;
using CompanyAPI.Domain.Exceptions;

namespace Enrollments.API.Controllers
{
    [Route("v{version:apiVersion}/enrollments")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ApiVersion("4.0")]
    [ApiController]
    public class EnrollmentController : ControllerBase
    {
        private readonly ILogger<EnrollmentController> _logger;
        private readonly ILoggerStateFactory _loggerStateFactory;
        private readonly IEnrollmentService _service;
        private readonly ITrainingProgramService _trainingProgramService;
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;
        private readonly Mapper _mapper_response;
        private readonly Mapper _mapper_request;

        public EnrollmentController(ILogger<EnrollmentController> logger, ILoggerStateFactory loggerStateFactory,
            IEnrollmentService service, ITrainingProgramService trainingProgramService, IUserService userService,
            IOrganizationService organizationService
        )
        {
            this._loggerStateFactory = loggerStateFactory;
            this._logger = logger;
            this._service = service;
            this._trainingProgramService = trainingProgramService;
            this._userService = userService;
            this._organizationService = organizationService;

            this._mapper_request = new Mapper(new MapperConfiguration(cfg => {
                cfg.CreateMap<EnrollmentCreateRequest, Enrollment>();
                cfg.CreateMap<EnrollmentUpdateRequest, Enrollment>();
                cfg.AllowNullCollections = true;
            }));
            this._mapper_response = new Mapper(new MapperConfiguration(cfg => cfg.CreateMap<Enrollment, EnrollmentResponse>()));
        }

        [HttpPut("generate-kafka-events")]
        [Authorize]
        [ProducesResponseType(202)]
        [ProducesResponseType(503)]
        public IActionResult GenerateKafkaEvents()
        {
            try
            {
                _service.GenerateKafkaEvents();
                return Accepted();
            }
            catch (GenerateKafkaEventsAlreadyRunningException)
            {
                return StatusCode(503, "Another process is already running");
            }
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(List<EnrollmentResponse>), 200)]
        [ProducesResponseType(typeof(BadRequestMessage), 400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllEnrollments(
            [Range(0, int.MaxValue)]
            [FromQuery] int offset = 0,
            [Range(1, 50)]
            [FromQuery] int size = 20,
            [FromQuery] int? userId = null,
            [FromQuery] string trainingProgramId = null
        )
        {
            using (_logger.BeginScope(_loggerStateFactory.Create(Request.Headers["Transaction-ID"])))
            {
                try {
                    if (!ModelState.IsValid)
                    {
                        return BadRequest(NonSuccessfullyRequestHelper.FormatBadRequestResponse(ModelState));
                    }

                    PageRequest pageRequest = new PageRequest();
                    pageRequest.PageOffset = offset;
                    pageRequest.PageSize = size;

                    List<Enrollment> enrollments = await _service.GetAllEnrollments(pageRequest, trainingProgramId: trainingProgramId, userId: userId);

                    List<EnrollmentResponse> response = enrollments.ConvertAll(enrollment => _mapper_response.Map<EnrollmentResponse>(enrollment));
                    response.ForEach(enrollment => enrollment._links.Self.Href = Url.Link("GetEnrollment", new { Id = enrollment.Id.ToString() }));

                    ReturnOutput<EnrollmentResponse> formattedEnrollment = new ReturnOutput<EnrollmentResponse>();
                    formattedEnrollment.Items = response;
                    formattedEnrollment.Count = _service.TotalFound;
                    formattedEnrollment.MakePaginationLinks(Request, pageRequest.PageOffset, pageRequest.PageSize, _service.TotalFound);

                    return Ok(formattedEnrollment);
                }
                catch(SystemException exception) {
                    _logger.LogError(exception, $"GetAllEnrollments - Unhandled Exception");
                    return StatusCode(500);
                }
            }
        }

        [HttpGet("{Id}", Name = "GetEnrollment")]
        [Authorize]
        [ProducesResponseType(typeof(EnrollmentResponse), 200)]
        [ProducesResponseType(typeof(BadRequestMessage), 400)]
        [ProducesResponseType(typeof(ResourceNotFoundMessage), 404)]
        public ActionResult GetEnrollment(
            [Required]
            [CustomIdFormatValidation]
            [FromRoute] Guid Id
        )
        {
            using (_logger.BeginScope(_loggerStateFactory.Create(Request.Headers["Transaction-ID"])))
            try {
                if (!ModelState.IsValid)
                {
                    return BadRequest(NonSuccessfullyRequestHelper.FormatBadRequestResponse(ModelState));
                }

                Enrollment enrollment = _service.GetEnrollment(Id);
                if (enrollment == null)
                {
                    return NotFound(NonSuccessfullyRequestHelper.FormatResourceNotFoundResponse());
                }

                EnrollmentResponse response = _mapper_response.Map<EnrollmentResponse>(enrollment);
                response._links.Self.Href = Url.Link("GetEnrollment", new { Id = response.Id.ToString() });
                return Ok(response);
            }
            catch(SystemException exception) {
                _logger.LogError(exception, $"GetEnrollment - Unhandled Exception");
                return StatusCode(500);
            }
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(EnrollmentResponse), 201)]
        [ProducesResponseType(typeof(BadRequestMessage), 400)]
        [ProducesResponseType(typeof(ConflictMessage), 409)]
        public async Task<IActionResult> CreateEnrollment(
            [FromBody] EnrollmentCreateRequest request)
        {
            using (_logger.BeginScope(_loggerStateFactory.Create(Request.Headers["Transaction-ID"])))
            try {
                if (!ModelState.IsValid) {
                    return BadRequest(NonSuccessfullyRequestHelper.FormatBadRequestResponse(ModelState));
                }

                Enrollment enrollment = _mapper_request.Map<Enrollment>(request);

                bool trainingProgramExists = await _trainingProgramService.CheckIfTrainingProgramExists(enrollment.TrainingProgramId.ToString());
                if (!trainingProgramExists)
                {
                    ModelState.AddModelError("training-program.id", "Training Program Not Found");
                    return BadRequest(NonSuccessfullyRequestHelper.FormatBadRequestResponse(ModelState));
                }

                bool userExists = await _userService.CheckIfUserExists(enrollment.UserId);
                if (!userExists)
                {
                    ModelState.AddModelError("user.id", "User Not Found");
                    return BadRequest(NonSuccessfullyRequestHelper.FormatBadRequestResponse(ModelState));
                }

                bool organizationExists = await _organizationService.CheckIfOrganizationExists(enrollment.OrganizationId);
                if (!organizationExists)
                {
                    ModelState.AddModelError("organization.id", "Organization Not Found");
                    return BadRequest(NonSuccessfullyRequestHelper.FormatBadRequestResponse(ModelState));
                }

                await _service.CreateEnrollment(enrollment);

                EnrollmentResponse response = _mapper_response.Map<EnrollmentResponse>(enrollment);
                response._links.Self.Href = Url.Link("GetEnrollment", new { Id = response.Id });

                var routeValues = new { response.Id };
                return CreatedAtRoute("GetEnrollment", routeValues, response);
            }
            catch (ResourceAlreadyExistsException exception)
            {
                _logger.LogInformation(exception, $"CreateEnrollment - Enrollment already exists");
                ConflictMessage message = new("An enrollment for the given user, organization and training program already exists.");
                return Conflict(message);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"CreateEnrollment - Unhandled Exception");
                return StatusCode(500);
            }
        }

        [HttpPatch("{Id}")]
        [Authorize]
        [ProducesResponseType(typeof(EnrollmentResponse), 200)]
        [ProducesResponseType(typeof(BadRequestMessage), 400)]
        [ProducesResponseType(typeof(ResourceNotFoundMessage),404)]
        public async Task<IActionResult> UpdateEnrollment(
            [Required]
            [CustomIdFormatValidation]
            [FromRoute] Guid Id,
            [Required]
            [FromBody] EnrollmentUpdateRequest request)
        {
            using (_logger.BeginScope(_loggerStateFactory.Create(Request.Headers["Transaction-ID"])))
            try {

                if (!ModelState.IsValid)
                {
                    return BadRequest(NonSuccessfullyRequestHelper.FormatBadRequestResponse(ModelState));
                }

                if(_service.GetEnrollment(Id) == null){
                    return NotFound(NonSuccessfullyRequestHelper.FormatResourceNotFoundResponse());
                }

                Enrollment enrollment = _mapper_request.Map<Enrollment>(request);


                enrollment = await _service.UpdateEnrollment(Id, enrollment);

                EnrollmentResponse response = _mapper_response.Map<EnrollmentResponse>(enrollment);
                response._links.Self.Href = Url.Link("GetEnrollment", new { Id = response.Id });

                return Ok(response);
            }
            catch(SystemException exception) {
                _logger.LogError(exception, $"UpdateEnrollment - Unhandled Exception");
                return StatusCode(500);
            }
        }

        [HttpDelete("{Id}")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(BadRequestMessage), 400)]
        [ProducesResponseType(typeof(ResourceNotFoundMessage),404)]
        public ActionResult DeleteEnrollment(
            [Required]
            [CustomIdFormatValidation]
            [FromRoute] Guid Id
       )
        {
            using (_logger.BeginScope(_loggerStateFactory.Create(Request.Headers["Transaction-ID"])))
            try {
                if (!ModelState.IsValid)
                {
                    return BadRequest(NonSuccessfullyRequestHelper.FormatBadRequestResponse(ModelState));
                }

                Enrollment enrollment = _service.DeleteEnrollment(Id);
                if (enrollment == null)
                {
                    return NotFound(NonSuccessfullyRequestHelper.FormatResourceNotFoundResponse());
                }

                return NoContent();
            }
            catch(SystemException exception) {
                _logger.LogError(exception, $"DeleteEnrollment - Unhandled Exception");
                return StatusCode(500);
            }
        }
    }
}
