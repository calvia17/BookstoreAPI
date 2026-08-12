using RabbitHoleService.Dtos;
using System.Text.Json;

namespace RabbitHoleService.Middleware
{
    /// <summary>
    /// The login details extraction middleware.
    /// </summary>
    public class LoginDetailsExtractionMiddleware
    {
        private readonly RequestDelegate next;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginDetailsExtractionMiddleware" /> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        public LoginDetailsExtractionMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        /// <summary>
        /// Invokes the middleware to extract login details from the request body.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>A task that represents the completion of the middleware execution.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.Equals("/api/auth/login", StringComparison.OrdinalIgnoreCase)
                && HttpMethods.IsPost(context.Request.Method))
            {
                // Read the request body.
                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                try
                {
                    var loginDto = JsonSerializer.Deserialize<LoginDto>(body, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    if (!string.IsNullOrEmpty(loginDto?.Email))
                    {
                        context.Items["LoginEmail"] = loginDto.Email;
                    }
                }
                catch
                {
                }
            }

            await this.next(context);
        }
    }
}
