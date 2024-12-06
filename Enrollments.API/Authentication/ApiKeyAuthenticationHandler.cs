using Amazon.Runtime.Internal;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using System.Linq;

namespace Enrollments.API.Authentication
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public ApiKeyAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("ApiKey", out var apiKeyHeaderValues)) {
                return Task.FromResult(AuthenticateResult.Fail("ApiKey not found in request headers."));
            }

            var apiKey = apiKeyHeaderValues.FirstOrDefault();

            if (apiKey == null) {
                return Task.FromResult(AuthenticateResult.Fail("Invalid ApiKey."));
            }

            var expectedApiKey = Environment.GetEnvironmentVariable("API_KEY");

            if (!string.Equals(apiKey, expectedApiKey)) {
                return Task.FromResult(AuthenticateResult.Fail("Invalid ApiKey."));
            }

            var identity = new ClaimsIdentity(
                new List<Claim> {
                    new Claim(ClaimTypes.Name, "ApiKeyUser")
                },
                Scheme.Name
            );

            var principal = new ClaimsPrincipal(identity);

            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
