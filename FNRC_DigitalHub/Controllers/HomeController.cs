using DigitalHub.Services.DTO;
using DigitalHub.Services.Interface;
using DigitalHub.Services.Services.IconConfig;
using FNRC_DigitalHub.Helper;
using FNRC_DigitalHub.Models;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FNRC_DigitalHub.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IIconConfigurationService iconConfigurationService;
        private readonly INotificationConfigurationService notificationConfigurationService;

        public HomeController(ILogger<HomeController> logger, IIconConfigurationService iconConfigurationService,
            INotificationConfigurationService notificationConfigurationService)
        {
            _logger = logger;
            this.iconConfigurationService = iconConfigurationService;
            this.notificationConfigurationService = notificationConfigurationService;
        }

        public async Task<IActionResult> IndexAsync()
        {
            var data = await iconConfigurationService.Get(0);
            return View(data);
        }

        public IActionResult Privacy()
        {
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> MarkNotificationAsRead([FromBody] NotificationUserDTO mod)
        {
            mod.Username = GetUser().userName;
            var result = await notificationConfigurationService.AddUserNotification(mod);
            return Json(new { success = true, message = result });
        }


        [HttpGet]
        public async Task<IActionResult> GetNotificationsToDisplay()
        {
            var user = GetUser();
            if (user != null)
            {
                var result = await notificationConfigurationService.GetNotificationToDisplay(user.userName);
                return Json(new { success = true, message = result });
            }
            return Json(new { success = true, message = "[]" });
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var user = GetUser();
            if (user != null)
            {
                var result = await notificationConfigurationService.Get(user.userName);
                return Json(new { success = true, message = result });
            }
            return Json(new { success = true, message = "[]" });
        }

        [HttpGet]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            if (string.IsNullOrEmpty(culture))
                culture = "ar"; // Default to English if no culture is provided

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );
            if (returnUrl.Contains("Error"))
            {
                return LocalRedirect("/");
            }
            return LocalRedirect(returnUrl ?? "/");
        }

        [HttpGet]
        public IActionResult GenerateSSOToken(string employeeId, string username, string displayName)
        {
            var tokenService = new SSOTokenService();
            var token = tokenService.GenerateToken(employeeId, username, displayName);
            return Json(new { token });
        }

        [HttpGet]
        public IActionResult SSOTokenTest(string employeeId)
        {
            try
            {
                var tokenService = new SSOTokenService();
                var token = tokenService.GenerateToken(employeeId, "username_test", "displayName_test", 960);
                return Json(new { token });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
