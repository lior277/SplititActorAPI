using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Splitit.ActorAPI.Web.ActorApi.Filter
{
    public class AuthorizeCheckOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Check if [Authorize] is applied at the controller or method level
            var hasAuthorize = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                                   .OfType<AuthorizeAttribute>().Any() ?? false ||
                               context.MethodInfo.GetCustomAttributes(true)
                                   .OfType<AuthorizeAttribute>().Any();

            if (hasAuthorize)
            {
                // Ensure the security collection is initialized
                operation.Security ??= new List<OpenApiSecurityRequirement>();

                // Add Bearer token security requirement
                operation.Security.Add(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "bearer",
                        Name = "Authorization",
                        In = ParameterLocation.Header
                    },
                    new List<string>() // No specific scopes
                }
            });
            }
        }
    }
}