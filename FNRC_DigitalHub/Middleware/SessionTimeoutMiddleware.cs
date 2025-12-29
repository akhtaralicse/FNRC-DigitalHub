
using DigitalHub.Domain.Enums;
using DigitalHub.Services.DTO; 

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
            //var user = context.Session.Get<UserSessionDTO>("UserSession");
            //var isAdmin = user?.Type.Any(x => x.Type == UserTypeEnum.Admin) ?? false;
            //var isDeptAdmin = user?.DepartmentAdminId>0;
            //var publicPaths = new[] { "/Account/Login", "/Account/UaePass", "/Home/Error", "/Home/Index", "/" };
            //var nonPublicPaths = new[] { "/ideas/submit", "/ideas", "/ideas/dashboard", "/dashboard/ViewDetail" };
            //var adminPublicPaths = new[] { "/admin" };
            //if (publicPaths.Any(path => context.Request.Path.StartsWithSegments(path)))
            //{
            //    await _next(context);
            //    return;
            //}
            //if (user == null && nonPublicPaths.Any(path => context.Request.Path.StartsWithSegments(path)))
            //{
            //    //  context.Response.Redirect("/Account/Login?timeout=true");  
            //    context.Response.Redirect("/");
            //    return;
            //}
            //if (adminPublicPaths.Any(path => context.Request.Path.StartsWithSegments(path)))
            //{
            //    if (isAdmin || isDeptAdmin)
            //    {
            //        await _next(context);
            //    }
            //    else
            //    {
            //        context.Response.Redirect("/");
            //        return;

            //    }
            //}else
             await _next(context);
        }
    }
}
