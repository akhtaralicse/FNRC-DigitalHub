using System.Text;

namespace FNRC_DigitalHub.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                // Read and log request headers
                var headers = context.Request.Headers
                    .Select(h => $"{h.Key}: {h.Value}")
                    .ToList();

                // Read and log request body
                context.Request.EnableBuffering();
                string body = "";
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
                {
                    body = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0; // Reset stream position
                }

                _logger.LogInformation("Request Received: \nHeaders: {Headers}\nBody: {Body}", string.Join("\n", headers), body);

                await _next(context); // Call next middleware
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request.");
                throw;
            }
        }
    }

}
