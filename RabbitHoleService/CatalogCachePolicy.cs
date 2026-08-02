using Azure;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OutputCaching;
using RabbitHoleService.Controllers;

namespace RabbitHoleService
{
    /// <summary>
    /// The catalog cache policy. 
    /// By default, output cache will not cache results if the request has an authentication header even
    /// if the endpoint does not require one. 
    /// This policy allows results to be cached even when an authentication header is present in the request.
    /// It is only meant to be used for endpoints that do not require authentication.
    /// </summary>
    public class CatalogCachePolicy : IOutputCachePolicy
    {
        private readonly TimeSpan expiration;

        /// <summary>
        /// Initializes a new instance of the <see cref="CatalogCachePolicy" /> class.
        /// </summary>
        /// <param name="expiration">The expiration.</param>
        /// <param name="tag">The tag.</param>
        public CatalogCachePolicy(TimeSpan expiration)
        {
            this.expiration = expiration;
        }

        /// <summary>
        /// Sets the cache request flags.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="cancellation">The cancellation token.</param>
        /// <returns>The task representing the operation.</returns>
        public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation)
        {
            var isCacheable = HttpMethods.IsGet(context.HttpContext.Request.Method);
            context.EnableOutputCaching = isCacheable;
            context.AllowCacheLookup = isCacheable;
            context.AllowCacheStorage = isCacheable;
            context.AllowLocking = true;
            context.ResponseExpirationTimeSpan = expiration;
            var actionName = context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>()?.ActionName;
            if (actionName == nameof(BookController.GetBook) && context.HttpContext.Request.RouteValues.TryGetValue("id", out var id))
            {
                context.Tags.Add($"book:{id}");
            }
            
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Serves from cache.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="cancellation">The cancellation token.</param>
        /// <returns>The task representing the operation.</returns>
        public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation) => ValueTask.CompletedTask;

        /// <summary>
        /// Serves the response.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="cancellation">The cancellation token.</param>
        /// <returns>The task representing the operation.</returns>
        public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation) => ValueTask.CompletedTask;

        /// <summary>
        /// Caches the response.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="cancellation">The cancellation token.</param>
        /// <returns>The task representing the operation.</returns>
        public ValueTask CacheResponseAsync(OutputCacheContext context, CancellationToken cancellation) => ValueTask.CompletedTask;
    }
}
