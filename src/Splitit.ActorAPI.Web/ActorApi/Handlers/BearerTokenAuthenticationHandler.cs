using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Splitit.ActorAPI.Web.ActorApi.Handlers
{
    public class BearerTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public BearerTokenAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Extract the Authorization header
            string authorizationHeader = Request.Headers["Authorization"];

            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                return Task.FromResult(AuthenticateResult.Fail("Missing or invalid Authorization header."));
            }

            // Extract the token from the header
            var token = authorizationHeader.Substring("Bearer ".Length).Trim();

            // Validate the token (Replace with your validation logic)
            if (string.IsNullOrEmpty(token) || token != "YourExpectedToken") // Replace "YourExpectedToken" with real validation
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid or expired token."));
            }

            // Create claims and authentication ticket
            var claims = new[] { new Claim(ClaimTypes.Name, "UserFromBearerToken") }; // Replace with actual claims
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
