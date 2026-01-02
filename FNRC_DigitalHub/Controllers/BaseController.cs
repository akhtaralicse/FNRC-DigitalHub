using DigitalHub.Services.DTO;
using FNRC_DigitalHub.Helper; 
using Microsoft.AspNetCore.Mvc;

namespace FNRC_DigitalHub.Controllers
{
    public class BaseController : Controller
    {
        public BaseController() { }

        public UserSessionDTO GetUser()
        {
            return HttpContext.Session.Get<UserSessionDTO>("UserSession");
        }
    }
}
