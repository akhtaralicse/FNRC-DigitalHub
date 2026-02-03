using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices;
using System.Security.Claims;

namespace FNRC_DigitalHub.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IConfiguration _config;
        public AccountController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            //if (User?.Identity?.IsAuthenticated == true)
            //{
            //    return LocalRedirect(returnUrl ?? Url.Content("~/"));
            //}

            ViewData["ReturnUrl"] = returnUrl ?? Url.Content("~/");
            return View(new CredentialInput());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(CredentialInput model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var username = model.Username?.Trim();
            var password = model.Password ?? string.Empty;

            var displayName = TryAuthenticateAndGetDisplayName(username, password);
            if (displayName == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim(ClaimTypes.Name, displayName),
                new Claim(ClaimTypes.Email, string.Empty)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProps = new AuthenticationProperties
            {
                IsPersistent = true
            };

            if (int.TryParse(_config["Session:ExpireDuration"], out var minutes) && minutes > 0)
            {
                authProps.ExpiresUtc = DateTime.UtcNow.AddMinutes(minutes);
            }

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);

            return LocalRedirect(  Url.Content("~/Account/Login"));
        }

        [HttpGet] 
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            HttpContext.Session.Clear(); 
            return RedirectToAction("Login");
        }

        private string TryAuthenticateAndGetDisplayName(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;

            var ldapPath = _config["LDAP:Domain"] ?? throw new InvalidOperationException("LDAP:Domain not configured.");

            var sam = username.Contains('\\') ? username.Split('\\').Last() : username;
            try
            {
                using var entry = new DirectoryEntry(ldapPath, username, password, AuthenticationTypes.Secure);
                // Force bind
                var native = entry.NativeObject;

                using var searcher = new DirectorySearcher(entry)
                {
                    Filter = $"(sAMAccountName={EscapeLdapSearchFilter(sam)})"
                };
                searcher.PropertiesToLoad.Add("displayName");
                var result = searcher.FindOne();
                if (result != null && result.Properties["displayName"]?.Count > 0)
                {
                    return result.Properties["displayName"][0]?.ToString() ?? sam;
                }
                return sam;
            }
            catch
            {
                return null;
            }
        }

        private static string EscapeLdapSearchFilter(string input)
        {
            return input?.Replace("\\", "\\5c").Replace("*", "\\2a").Replace("(", "\\28").Replace(")", "\\29").Replace("\0", "\\00") ?? string.Empty;
        }
    }

    public class CredentialInput
    {
        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}