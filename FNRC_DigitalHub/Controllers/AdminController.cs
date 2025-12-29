using System.Diagnostics;
using System.Threading.Tasks;
using AutoMapper.Configuration.Annotations;
using DigitalHub.Domain.Enums;
using DigitalHub.Services.DTO;
using DigitalHub.Services.Interface;
using DigitalHub.Services.Services;
using DigitalHub.Services.Services.IconConfig;
using FNRC_DigitalHub.Models;
using Microsoft.AspNetCore.Mvc;

namespace FNRC_DigitalHub.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IIconConfigurationService iconConfigurationService;
        private readonly INotificationConfigurationService notificationConfigurationService;

        public AdminController(ILogger<HomeController> logger, IIconConfigurationService iconConfigurationService
            , INotificationConfigurationService notificationConfigurationService)
        {
            _logger = logger;
            this.iconConfigurationService = iconConfigurationService;
            this.notificationConfigurationService = notificationConfigurationService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> IconSetting()
        {
            var data = await iconConfigurationService.GetByType(IconTypeEnum.Icon);

            return View(data);
        }
        public async Task<IActionResult> VideoSetting()
        {
            var data = await iconConfigurationService.GetByType(IconTypeEnum.BGVideo);

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddIcons(IconConfigurationDTO mod)
        {
            var data = await iconConfigurationService.Add(mod);

            return Json(new { success = true, message = "Saved" });
        }
        public async Task<IActionResult> UpdateIcons(IconConfigurationDTO mod)
        {
            var data = await iconConfigurationService.Update(mod);

            return Json(new { success = true, message = "Updated" });
        }
        public async Task<IActionResult> Announcement()
        {
            var data = await notificationConfigurationService.Get();

            return View(data);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
