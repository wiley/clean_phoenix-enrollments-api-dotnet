using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System.Collections.Generic;

using Enrollments.Services.Interfaces;
using Enrollments.API.Responses;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Enrollments.API.Requests.Event;
using System;
using Enrollments.Domain.Pagination;
using Enrollments.Domain.Params;
using Enrollments.Domain.Event;
using Enrollments.API.Helpers;
using Enrollments.Domain;
using WLS.Log.LoggerTransactionPattern;
using Microsoft.AspNetCore.Authorization;

namespace Enrollments.API.Controllers
{
    [Route("v{version:apiVersion}/events")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ApiVersion("4.0")]
    [ApiController]
    public class EventController: ControllerBase
    {
        private readonly ILogger<EventController> _logger;
        private readonly ILoggerStateFactory _loggerStateFactory;
        private readonly IEventService _service;
        private readonly ITrainingProgramService _trainingProgramService;
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;
        private readonly Mapper _mapper_response;
        private readonly Mapper _mapper_request;

        public EventController(
            ILogger<EventController> logger,
            ILoggerStateFactory loggerStateFactory,
            IEventService service,
            ITrainingProgramService trainingProgramService,
            IUserService userService,
            IOrganizationService organizationService
        )
        {
            this._loggerStateFactory = loggerStateFactory;
            this._logger = logger;
            this._service = service;
            this._trainingProgramService = trainingProgramService;
            this._userService = userService;
            this._organizationService = organizationService;

            this._mapper_request = new Mapper(
                new MapperConfiguration(configuration => {
                    configuration.CreateMap<EventCreateRequest, Event>();
                    configuration.CreateMap<EventUpdateRequest, Event>();
                    configuration.AllowNullCollections = true;
                })
            );
            this._mapper_response = new Mapper(
                new MapperConfiguration(configuration => configuration.CreateMap<Event, EventResponse>())
            );
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(List<EventResponse>), 200)]
        [ProducesResponseType(typeof(BadRequestMessage), 400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllEvents(
            [FromQuery] EventFilterParams filterParams,
            [FromQuery] PaginationParams paginationParams
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
                    pageRequest.PageOffset = paginationParams.offset;
                    pageRequest.PageSize = paginationParams.size;

                    List<Event> events = await _service.GetAllEvents(
                        pageRequest,
                        filterParams
                    );

                    List<EventResponse> response = events.ConvertAll(theEvent => _mapper_response.Map<EventResponse>(theEvent));
                    response.ForEach(
                        theEvent => theEvent._links.Self.Href = Url.Link(
                            "GetEvent",
                            new { Id = theEvent.Id.ToString() }
                        )
                    );

                    ReturnOutput<EventResponse> formattedEvent = new ReturnOutput<EventResponse>();
                    formattedEvent.Items = response;
                    formattedEvent.Count = _service.TotalFound;
                    formattedEvent.MakePaginationLinks(Request, pageRequest.PageOffset, pageRequest.PageSize, _service.TotalFound);

                    return Ok(formattedEvent);
                } catch(SystemException exception) {
                    _logger.LogError(exception, $"GetAllEvents - Unhandled Exception");
                    return StatusCode(500);
                }
            }
        }

        [HttpGet("{Id}", Name = "GetEvent")]
        [Authorize]
        [ProducesResponseType(typeof(EventResponse), 200)]
        [ProducesResponseType(typeof(BadRequestMessage), 400)]
        [ProducesResponseType(typeof(ResourceNotFoundMessage), 404)]
        public ActionResult GetEvent(
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

                Event theEvent = _service.GetEvent(Id);
                if (theEvent == null)
                {
                    return NotFound(NonSuccessfullyRequestHelper.FormatResourceNotFoundResponse());
                }

                EventResponse response = _mapper_response.Map<EventResponse>(theEvent);
                response._links.Self.Href = Url.Link("GetEvent", new { Id = response.Id.ToString() });
                return Ok(response);
            } catch(SystemException exception) {
                _logger.LogError(exception, $"GetEvent - Unhandled Exception");
                return StatusCode(500);
            }
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(EventResponse), 201)]
        [ProducesResponseType(typeof(BadRequestMessage), 400)]
        public async Task<IActionResult> CreateEvent(
            [FromBody] EventCreateRequest request)
        {
            using (_logger.BeginScope(_loggerStateFactory.Create(Request.Headers["Transaction-ID"])))
            try {
                if (!ModelState.IsValid) {
                    return BadRequest(NonSuccessfullyRequestHelper.FormatBadRequestResponse(ModelState));
                }

                Event theEvent = _mapper_request.Map<Event>(request);

                await _service.CreateEvent(theEvent);

                EventResponse response = _mapper_response.Map<EventResponse>(theEvent);
                response._links.Self.Href = Url.Link("GetEvent", new { Id = response.Id });

                var routeValues = new { response.Id };

                return CreatedAtRoute("GetEvent", routeValues, response);
            } catch(SystemException exception) {
                _logger.LogError(exception, $"CreateEvent - Unhandled Exception");

                return StatusCode(500);
            }
        }

        [HttpPatch("{Id}")]
        [Authorize]
        [ProducesResponseType(typeof(EventResponse), 200)]
        [ProducesResponseType(typeof(BadRequestMessage), 400)]
        [ProducesResponseType(typeof(ResourceNotFoundMessage),404)]
        public async Task<IActionResult> UpdateEvent(
            [Required]
            [CustomIdFormatValidation]
            [FromRoute] Guid Id,
            [Required]
            [FromBody] EventUpdateRequest request)
        {
            using (_logger.BeginScope(_loggerStateFactory.Create(Request.Headers["Transaction-ID"])))
            try {

                if (!ModelState.IsValid)
                {
                    return BadRequest(NonSuccessfullyRequestHelper.FormatBadRequestResponse(ModelState));
                }

                if(_service.GetEvent(Id) == null){
                    return NotFound(NonSuccessfullyRequestHelper.FormatResourceNotFoundResponse());
                }

                Event theEvent = _mapper_request.Map<Event>(request);

                theEvent = await _service.UpdateEvent(Id, theEvent);

                EventResponse response = _mapper_response.Map<EventResponse>(theEvent);
                response._links.Self.Href = Url.Link("GetEvent", new { Id = response.Id });

                return Ok(response);
            } catch(SystemException exception) {
                _logger.LogError(exception, $"UpdateEvent - Unhandled Exception");

                return StatusCode(500);
            }
        }

        [HttpDelete("{Id}")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(BadRequestMessage), 400)]
        [ProducesResponseType(typeof(ResourceNotFoundMessage),404)]
        public ActionResult DeleteEvent(
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

                Event theEvent = _service.DeleteEvent(Id);
                if (theEvent == null)
                {
                    return NotFound(NonSuccessfullyRequestHelper.FormatResourceNotFoundResponse());
                }

                return NoContent();
            } catch(SystemException exception) {
                _logger.LogError(exception, $"DeleteEvent - Unhandled Exception");

                return StatusCode(500);
            }
        }
    }
}
