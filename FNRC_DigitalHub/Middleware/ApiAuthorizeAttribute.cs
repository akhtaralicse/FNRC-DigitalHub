using Microsoft.AspNetCore.Authorization; 
namespace FNRC_DigitalHub.Middleware
{ 
    public class ApiAuthorizeAttribute : AuthorizeAttribute
    {
        public ApiAuthorizeAttribute()
        {
            AuthenticationSchemes = "JwtAuth";
        }
    }

}
