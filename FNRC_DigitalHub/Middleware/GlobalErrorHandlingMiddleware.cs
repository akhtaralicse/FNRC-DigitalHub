using System.Text;

namespace FNRC_DigitalHub.Middleware
{
    public class GlobalErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalErrorHandlingMiddleware> _logger;

        public GlobalErrorHandlingMiddleware(RequestDelegate next, ILogger<GlobalErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An unhandled exception occurred.");

                await LogRequest(context, ex); // Log request details on error
                //throw; // Re-throw the exception after logging

                //bool isPartialViewRequest = IsPartialViewRequest(context.Request);

                //if (!isPartialViewRequest)
                //{
                //    // Redirect to the error page for normal views
                //    // context.Response.Redirect("/Home/Error");
                //    context.Response.Redirect("/");
                //}
                //else
                //{
                //    // For partial views, return an error response without redirecting
                //    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                //    context.Response.ContentType = "text/plain";
                //    await context.Response.WriteAsync("An error occurred while processing your request.");
                //}

            }
        }
        private async Task LogRequest(HttpContext context, Exception ex)
        {
            var request = context.Request;

            // Read headers
            var headers = string.Join("\n", request.Headers.Select(h => $"{h.Key}: {h.Value}"));

            // Read request body
            context.Request.EnableBuffering();
            string body = "";
            using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0; // Reset stream position
            }

            _logger.LogError(ex, "HTTP Request Error:\nMethod: {Method}\nPath: {Path}\nHeaders: {Headers}\nBody: {Body}",
                request.Method, request.Path, headers, body);
        }
        private bool IsPartialViewRequest(HttpRequest request)
        {
            // Check if the request is an AJAX request or has a specific header for partial views
            if (request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                request.Path.StartsWithSegments("/Partial", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
