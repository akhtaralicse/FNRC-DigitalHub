using DigitalHub.Domain.Enums;
using DigitalHub.Services.DTO;
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
    public class WhoAmIController(IConfiguration config, IHostEnvironment environment) : BaseController
    {
        public IConfiguration Config { get; } = config;
        public IHostEnvironment Environment { get; } = environment;

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUserAsync()
        {

            if (Environment.IsDevelopment())
            {
                var r = await CreateUserSessionAsync("TestUser", "TestUser", "TestUser", 0, null, "", "");
                if (r)
                    return Ok(new { UserName = "TestUser", DisplayName = "TestUser" });
            }

            var identity = HttpContext.User?.Identity;
            if (identity == null || !identity.IsAuthenticated)
            {
                await CreateUserSessionAsync(null, null, null, 0, null, "", "");
                return Unauthorized();
            }
            var domainUser = identity.Name ?? string.Empty;
            var samAccountName = domainUser.Contains('\\') ? domainUser.Split('\\')[1] : domainUser;

            var (DisplayName, EmployeeId) = GetUserDetailsFromAd(samAccountName);
            ICollection<UserTypeDTO> UserType = [new()
            {
                Type = UserTypeEnum.Employee
            }];
             
            var claims = new List<Claim>
                        {
                            new(ClaimTypes.Name, samAccountName),
                            new("EmployeeId", EmployeeId ?? string.Empty),
                            new("DisplayName", DisplayName), 
                            new("UserType", string.Join(",", UserType.Select(u => u.Type.ToString())))                            
                        };

            //var claimsIdentity = new ClaimsIdentity(claims, "DigitalHubCookie");
         
            //var authProperties = new AuthenticationProperties
            //{
            //    IsPersistent = true,
            //    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(12)
            //};

            //await HttpContext.SignInAsync("DigitalHubCookie", new ClaimsPrincipal(claimsIdentity), authProperties);

            var res = await CreateUserSessionAsync(samAccountName, DisplayName, DisplayName, int.Parse(EmployeeId), UserType, "", "");

            return Ok(new { UserName = samAccountName, DisplayName, EmployeeId });
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
                       // new (ClaimTypes.Role, data.Type != null ? data.Type?.ToString():null),
                       // new  (ClaimTypes.Authentication, data.Token),
                        new  ("Type",  data.Type != null ? data.Type?.ToString():""),
                        //new  ("UserID", data.User.Id.ToString()),
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity)
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
