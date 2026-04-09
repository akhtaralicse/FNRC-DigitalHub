using DigitalHub.Domain.Enums;
using DigitalHub.Services.DTO;
using DigitalHub.Services.Interface;
using FNRC_DigitalHub.Helper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices;
using System.Security.Claims;
using DirectoryEntry = System.DirectoryServices.DirectoryEntry;

namespace FNRC_DigitalHub.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WhoAmIController(IConfiguration config, IHostEnvironment environment, IUserService userService) : BaseController
    {
        public IConfiguration Config { get; } = config;
        public IHostEnvironment Environment { get; } = environment;
        private readonly IUserService _userService = userService;

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUserAsync(string returnUrl = null)
        {
            //if (Environment.IsDevelopment())
            //{
            //    var usera = await _userService.GetUserByUsername("TestUser");
            //    if (usera != null)
            //    {
            //        await CreateUserSessionAsync("TestUser", "TestUser", "TestUser", 74215, usera.UserType, usera.MobileNo, "");
            //        if (!string.IsNullOrEmpty(returnUrl)) return LocalRedirect(returnUrl);
            //        return Ok(new { UserName = "TestUser", DisplayName = "TestUser", EmployeeId = "74215" });
            //    }
            //    else
            //    {
            //        var role = new List<UserTypeDTO>
            //        {
            //            new UserTypeDTO { Type = UserTypeEnum.Employee }
            //        };
            //        var r = await CreateUserSessionAsync("TestUser", "TestUser", "TestUser", 74215, role, "", "");
            //        var user1 = await _userService.GetOrRegisterUser(74215, "TestUser", "TestUser@fnrc.gov.ae");
            //        if (r)
            //        {
            //            if (!string.IsNullOrEmpty(returnUrl)) return LocalRedirect(returnUrl);
            //            return Ok(new { UserName = "TestUser", DisplayName = "TestUser", EmployeeId = "74215" });
            //        }
            //    }
            //}

            var identity = HttpContext.User?.Identity;
            if (identity == null || !identity.IsAuthenticated)
            {
                await CreateUserSessionAsync(null, null, null, 0, null, "", "");
                return Unauthorized();
            }
            var domainUser = identity.Name ?? string.Empty;
            var samAccountName = domainUser.Contains('\\') ? domainUser.Split('\\')[1] : domainUser;

            var user = await _userService.GetUserByUsername(samAccountName);

            if (user == null)
            {
                var (DisplayName, EmployeeId) = GetUserDetailsFromAd(samAccountName);
                if (string.IsNullOrEmpty(EmployeeId)) return Unauthorized("Employee ID not found in AD.");

                user = await _userService.GetOrRegisterUser(int.Parse(EmployeeId), DisplayName, samAccountName);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, samAccountName),
                new("EmployeeId", user.EmployeeId.ToString()),
                new("DisplayName", user.NameEn),
                new("UserType", string.Join(",", user.UserType.Select(u => u.Type.ToString())))
            };

            await CreateUserSessionAsync(samAccountName, user.NameEn, user.NameAr, user.EmployeeId, user.UserType, user.MobileNo, samAccountName + "@fnrc.gov.ae");

            if (!string.IsNullOrEmpty(returnUrl)) return LocalRedirect(returnUrl);
            return Ok(new { UserName = samAccountName, DisplayName = user.NameEn, EmployeeId = user.EmployeeId.ToString() });
        }

        private (string DisplayName, string EmployeeId) GetUserDetailsFromAd(string samAccountName)
        {
            string ldapPath = Config["LDAP:Domain"].ToString();
            string employeeID = Config["LDAP:EmployeeIDText"].ToString();
            using var entry = new DirectoryEntry(ldapPath);
            using var searcher = new DirectorySearcher(entry)
            {
                Filter = $"(sAMAccountName={EscapeLdapSearchFilter(samAccountName)})"
            };

            // Load both properties
            searcher.PropertiesToLoad.Add("displayName");
            searcher.PropertiesToLoad.Add(employeeID);

            var res = searcher.FindOne();

            if (res != null)
            {
                string displayName = res.Properties.Contains("displayName") ? res.Properties["displayName"][0].ToString() : samAccountName;

                string employeeId = res.Properties.Contains(employeeID) ? res.Properties[employeeID][0].ToString() : null;

                return (displayName, employeeId);
            }

            return (samAccountName, null);
        }

        // Basic escaping for LDAP filter control characters
        private string EscapeLdapSearchFilter(string input)
        {
            return input.Replace("\\", "\\5c").Replace("*", "\\2a").Replace("(", "\\28").Replace(")", "\\29").Replace("\0", "\\00");
        }
        private async Task<bool> CreateUserSessionAsync(string username, string nameEn, string nameAr, int employeeId, ICollection<UserTypeDTO> userTypes, string mobileNo, string email)
        {

            var userSession = new UserSessionDTO(username, nameEn, nameAr, employeeId, userTypes, mobileNo, email);
            await AddClaims(userSession);
            HttpContext.Session.Set("UserSession", userSession);
            return true;
        }
        private async Task<bool> AddClaims(UserSessionDTO data)
        {
            try
            {
                if (data.NameAr != null)
                {
                    var claims = new List<Claim>
                    {
                        new (ClaimTypes.Email, data. Email),
                        new (ClaimTypes.NameIdentifier, data. EmployeeID.ToString()),
                        new (ClaimTypes.Name, data. NameEn),
                        new  ("Type",  data.Type != null ? string.Join(",", data.Type.Select(x=>x.Type.ToString())):""),
                    };

                    if (data.Type != null && data.Type.Any())
                    {
                        foreach (var role in data.Type)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role.Type.ToString()));
                        }
                    }
                    else
                    {
                        claims.Add(new Claim(ClaimTypes.Role, UserTypeEnum.Employee.ToString()));
                    }
                    var claimsIdentity = new ClaimsIdentity(claims, "DigitalHubCookie");
                    await HttpContext.SignInAsync("DigitalHubCookie", new ClaimsPrincipal(claimsIdentity)
                    , new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTime.UtcNow.AddMinutes(double.Parse(Config["Session:ExpireDuration"]))
                    });
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
            return false;
        }
    }
}
