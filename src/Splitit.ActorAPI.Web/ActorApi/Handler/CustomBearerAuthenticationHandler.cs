﻿using Microsoft.AspNetCore.Authentication;
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
            UrlEncoder encoder)
            : base(options, logger, encoder)
        { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);

            if (string.IsNullOrEmpty(token))
            {
                return Task.FromResult(AuthenticateResult.Fail("Authorization token is missing or invalid."));
            }

            var ticket = new AuthenticationTicket(new ClaimsPrincipal(), "Bearer");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
