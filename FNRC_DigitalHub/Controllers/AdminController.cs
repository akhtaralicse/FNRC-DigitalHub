using AutoMapper.Configuration.Annotations;
using ClosedXML.Excel;
using DigitalHub.Domain.Enums;
using DigitalHub.Services.DTO;
using DigitalHub.Services.Interface;
using DigitalHub.Services.Services;
using DigitalHub.Services.Services.IconConfig;
using FNRC_DigitalHub.Models;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace FNRC_DigitalHub.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration configuration;
        private readonly IIconConfigurationService iconConfigurationService;
        private readonly INotificationConfigurationService notificationConfigurationService;
        private readonly IIconConfigurationAttachmentService iconConfigurationAttachmentService;
        private readonly IUserRoleService userRoleService;

        public AdminController(ILogger<HomeController> logger, IConfiguration configuration, IIconConfigurationService iconConfigurationService
            , INotificationConfigurationService notificationConfigurationService,
            IIconConfigurationAttachmentService iconConfigurationAttachmentService,
            IUserRoleService userRoleService)
        {
            _logger = logger;
            this.configuration = configuration;
            this.iconConfigurationService = iconConfigurationService;
            this.notificationConfigurationService = notificationConfigurationService;
            this.iconConfigurationAttachmentService = iconConfigurationAttachmentService;
            this.userRoleService = userRoleService;
        }

        public IActionResult Index()
        {
            return View();
        }


        #region Icon and video--------------------
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
        // [RequestSizeLimit(40 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 40 * 1024 * 1024)]
        public async Task<IActionResult> AddIcons(IconConfigurationDTO mod)
        {
            var data = await iconConfigurationService.Add(mod);

            return Json(new { success = true, message = "Saved" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(MultipartBodyLengthLimit = 40 * 1024 * 1024)]
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
        #endregion


        #region Notification ------------------------------
        public async Task<IActionResult> Announcement()
        {
            //var data = await notificationConfigurationService.GetAll();

            return View();
        }

        public async Task<IActionResult> GetNotificationById(int id)
        {
            var data = await notificationConfigurationService.GetById(id);

            return Json(new { success = true, message = data });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNotification(NotificationConfigurationDTO mod)
        {
            var data = await notificationConfigurationService.Add(mod);

            return Json(new { success = true, message = "Saved" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateNotification(NotificationConfigurationDTO mod)
        {
            var data = await notificationConfigurationService.Update(mod);

            return Json(new { success = true, message = "Saved" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var data = await notificationConfigurationService.Delete(id);

            return Json(new { success = true, message = "Saved" });
        }

        [HttpGet]
        public async Task<IActionResult> ExportAnnouncementsToExcel(string name, DateTime? dateFrom, DateTime? dateTo)
        {
            var data = await notificationConfigurationService.GetBySearch(name, dateFrom, dateTo);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Announcements");
                worksheet.Cell(1, 1).Value = "Title (EN)";
                worksheet.Cell(1, 2).Value = "Title (AR)";
                worksheet.Cell(1, 3).Value = "Message (EN)";
                worksheet.Cell(1, 4).Value = "Message (AR)";
                worksheet.Cell(1, 5).Value = "Start Date";
                worksheet.Cell(1, 6).Value = "End Date";
                worksheet.Cell(1, 7).Value = "Action URL";
                worksheet.Cell(1, 8).Value = "Action Text (EN)";
                worksheet.Cell(1, 9).Value = "Action Text (AR)";
                for (int i = 0; i < data.Count; i++)
                {
                    var announcement = data[i];
                    int row = i + 2;

                    worksheet.Cell(row, 1).Value = announcement.TitleEn;
                    worksheet.Cell(row, 2).Value = announcement.TitleAr;
                    worksheet.Cell(row, 3).Value = announcement.MessageEn;
                    worksheet.Cell(row, 4).Value = announcement.MessageAr;

                    worksheet.Cell(row, 5).Value = announcement.StartDate;
                    worksheet.Cell(row, 6).Value = announcement.EndDate;

                    worksheet.Cell(row, 7).Value = announcement.ActionUrl;
                    worksheet.Cell(row, 8).Value = announcement.ActionTextEn;
                    worksheet.Cell(row, 9).Value = announcement.ActionTextAr;
                }
                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                var content = stream.ToArray();

                string excelName = $"Announcements_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }

        }

        public async Task<IActionResult> SearchAnnouncements(string name, DateTime? dateFrom, DateTime? dateTo, int page = 1, int pageSize = 10)
        {
            int skip = (page - 1) * pageSize;
            var data = await notificationConfigurationService.GetBySearch(name, dateFrom, dateTo, pageSize, skip);
            var paginatedData = data.Skip(skip).Take(pageSize).ToList();
            int totalCount = data.Count;

            return Json(new
            {
                success = true,
                data = paginatedData,
                totalCount,
                currentPage = page,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }
        #endregion

        #region User Role Management ------------------------------
        public async Task<IActionResult> UserRoleManage()
        {
            var users = await userRoleService.GetAllUsers();
            var roles = Enum.GetValues(typeof(UserTypeEnum))
                            .Cast<UserTypeEnum>()
                            .Select(e => new
                            {
                                Id = (int)e,
                                Name = e.ToString(), // Keep enum name for mapping
                                DisplayName = typeof(UserTypeEnum).GetMember(e.ToString()).FirstOrDefault()?.GetCustomAttribute<DisplayAttribute>()?.Name ?? e.ToString()
                            })
                            .ToList();

            ViewBag.Users = users;
            ViewBag.Roles = roles;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAssignedRoles()
        {
            var data = await userRoleService.GetUsersWithRoles();
            return Json(new { success = true, data });
        }

        [HttpGet]
        public async Task<IActionResult> GetUserRoles(int userId)
        {
            var data = await userRoleService.GetUserRoles(userId);
            return Json(new { success = true, data });
        }

        [HttpPost]
        public async Task<IActionResult> AssignUserRoles(int userId, List<int> roles)
        {
            var result = await userRoleService.AssignRoles(userId, roles);
            return Json(new { success = result, message = result ? "Roles assigned successfully" : "Failed to assign roles" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUserRoles(int userId)
        {
            var result = await userRoleService.AssignRoles(userId, new List<int>()); // Assign empty list to remove all
            return Json(new { success = result, message = result ? "Roles removed successfully" : "Failed to remove roles" });
        }
        #endregion


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
