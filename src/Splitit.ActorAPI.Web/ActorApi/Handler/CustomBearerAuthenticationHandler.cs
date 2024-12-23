using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Splitit.ActorAPI.Web.ActorApi.Handler
{
    public class CustomBearerAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public CustomBearerAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);

            // Simple validation logic or custom processing
            if (string.IsNullOrEmpty(token))
            {
                return Task.FromResult(AuthenticateResult.Fail("Authorization token is missing or invalid."));
            }

            // For example, check if the token is the correct format or matches a known value
            var claims = new[]
            {
            new Claim(ClaimTypes.Name, "AuthenticatedUser"),
            // You can add more claims based on your requirements
        };

            var identity = new ClaimsIdentity(claims, "Bearer");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Bearer");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}