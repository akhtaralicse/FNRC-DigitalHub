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
        private readonly IIconConfigurationAttachmentService iconConfigurationAttachmentService;

        public AdminController(ILogger<HomeController> logger, IIconConfigurationService iconConfigurationService
            , INotificationConfigurationService notificationConfigurationService,
            IIconConfigurationAttachmentService  iconConfigurationAttachmentService)
        {
            _logger = logger;
            this.iconConfigurationService = iconConfigurationService;
            this.notificationConfigurationService = notificationConfigurationService;
            this.iconConfigurationAttachmentService = iconConfigurationAttachmentService;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateIcons(IconConfigurationDTO mod)
        {
            var data = await iconConfigurationService.Update(mod);

            return Json(new { success = true, message = "Updated" });
        }
        [HttpPost]
        public async Task<IActionResult> DeleteIcon(int id)
        {
            var result = await iconConfigurationService.Delete(id);
            var result2 = await iconConfigurationAttachmentService.DeleteAttachment(id);
            return Json(new { success = result2, message = "Deleted" });

        }
        [HttpPost]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var result = await iconConfigurationAttachmentService.DeleteAttachment(id);
            return Json(new { success = result, message = "Deleted" });

        }
        public async Task<IActionResult> GetIconById(int id)
        {
            var data = await iconConfigurationService.Get(id);

            return Json(new { success = true, message = data });
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
