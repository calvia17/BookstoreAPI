using Microsoft.AspNetCore.Authorization;
using RabbitHoleService.Services;
using System.IdentityModel.Tokens.Jwt;

namespace RabbitHoleService.Middleware
{
    /// <summary>
    /// The token blacklist middleware.
    /// </summary>
    public class TokenBlacklistMiddleware
    {
        private readonly RequestDelegate next;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenBlacklistMiddleware" /> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        public TokenBlacklistMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        /// <summary>
        /// Invokes the middleware to check if the JWT token is blacklisted.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="tokenCacheService">The token cache service.</param>
        /// <returns>A task that represents the completion of the middleware execution.</returns>
        public async Task InvokeAsync(HttpContext context, ITokenCacheService tokenCacheService)
        {
            if (context.User.Identity?.IsAuthenticated == true && context.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() == null)
            {
                var jtiClaim = context.User.FindFirst(JwtRegisteredClaimNames.Jti);
                if (jtiClaim == null || !Guid.TryParse(jtiClaim.Value, out var jwtId))
                {
                    await WriteUnauthorizedResponseAsync(context);
                    return;
                }

                var isBlacklisted = await tokenCacheService.IsTokenBlacklistedAsync(jwtId, context.RequestAborted);
                if (isBlacklisted)
                {
                    await WriteUnauthorizedResponseAsync(context);
                    return;
                }
            }

            await this.next(context);
        }

        private static async Task WriteUnauthorizedResponseAsync(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "Invalid token" }, context.RequestAborted);
        }
    }
}
