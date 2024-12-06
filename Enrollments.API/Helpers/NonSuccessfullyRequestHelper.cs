using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Enrollments.API.Helpers
{
    public static class NonSuccessfullyRequestHelper
    {
        private static readonly string REQUEST_CONTAINS_INVALID_DATA = "The request contains invalid data.";
        private static readonly string ERROR_ORIGIN_BODY = "body";
        private static readonly string ERROR_TYPE_INVALID_VALUE = "errors/invalid-value";

        public static BadRequestMessage FormatBadRequestResponse(ModelStateDictionary modelState)
        {
            BadRequestMessage badRequestMessage = new();

            badRequestMessage.Message = REQUEST_CONTAINS_INVALID_DATA;

            foreach (string modelStateKey in modelState.Keys)
                foreach (var error in modelState[modelStateKey].Errors)
                    badRequestMessage.Errors.Add(new Error(modelStateKey, ERROR_ORIGIN_BODY, ERROR_TYPE_INVALID_VALUE, error.ErrorMessage));

            return badRequestMessage;
        }

        public static ResourceNotFoundMessage FormatResourceNotFoundResponse() {
            return new ResourceNotFoundMessage();
        }
    }

    public class ResourceNotFoundMessage {
        private static readonly string RESOURCE_NOT_FOUND = "The requested resource does not exist.";
        public string Message { get; set;}

        public ResourceNotFoundMessage() {
            Message = RESOURCE_NOT_FOUND;
        }

        public ResourceNotFoundMessage(string message) {
            Message = message;
        }
    }

    public class BadRequestMessage
    {
        public string Message { get; set; }
        public List<Error> Errors { get; set; }

        public BadRequestMessage()
        {
            Errors = new List<Error>();
        }
    }

    public class Error
    {
        public string Name { get; set; }

        public string Origin { get; set; }

        public string Type { get; set; }

        public string Detail { get; set; }

        public Error(string name, string origin, string type, string detail)
        {
            Name = name;
            Origin = origin;
            Type = type;
            Detail = detail;
        }
    }

    public class ConflictMessage
    {
        private static readonly string CONFLICT_MESSAGE = "A conflict occured while processing the request.";
        public string Message { get; set; }

        public ConflictMessage() {
            Message = CONFLICT_MESSAGE;
        }

        public ConflictMessage(string message) {
            Message = message;
        }
    }
}
