
using DigitalHub.Domain.Enums;
using DigitalHub.Services.DTO;
using FNRC_DigitalHub.Helper;

namespace FNRC_DigitalHub.Middleware
{
    public class SessionTimeoutMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionTimeoutMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var adminPublicPaths = new[] { "/admin" };
            var user = context.Session.Get<UserSessionDTO>("UserSession");

            if (adminPublicPaths.Any(path => context.Request.Path.StartsWithSegments(path)))
            {
                if (user != null && user?.EmployeeID > 0)
                {
                    var isAdmin = user?.Type.Count(z => z.Type != UserTypeEnum.Employee) > 0;
                    if (isAdmin)
                    {
                        await _next(context);
                    }
                }
                else
                {
                    context.Response.Redirect("/");
                    return;
                }
            }
            else
                await _next(context);
        }
    }
}
