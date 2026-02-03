using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.DirectoryServices;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace FNRC_DigitalHub.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IConfiguration _config;
        public LoginModel(IConfiguration config)
        {
            _config = config;
        }

        [BindProperty]
        public CredentialInput Input { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            if (User?.Identity?.IsAuthenticated == true)
            {
                // already signed in -> redirect
                Response.Redirect(ReturnUrl);
            }
        }

        public async Task<IActionResult> OnPostLoginAsync()
        {
            ReturnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var username = Input.Username?.Trim();
            var password = Input.Password ?? string.Empty;

            // Attempt AD bind/search using provided credentials
            var displayName = TryAuthenticateAndGetDisplayName(username, password);
            if (displayName == null)
            {
                ErrorMessage = "Invalid username or password.";
                return Page();
            }

            // Create claims and sign in using cookie auth
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim(ClaimTypes.Name, displayName),
                new Claim(ClaimTypes.Email, "")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProps = new AuthenticationProperties
            {
                IsPersistent = true,
            };

            if (int.TryParse(_config["Session:ExpireDuration"], out var minutes) && minutes > 0)
            {
                authProps.ExpiresUtc = DateTime.UtcNow.AddMinutes(minutes);
            }

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);

            return LocalRedirect(ReturnUrl);
        }

        private string TryAuthenticateAndGetDisplayName(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;
            var ldapPath = _config["LDAP:Domain"] ?? throw new InvalidOperationException("LDAP:Domain not configured.");

            // If username was provided as DOMAIN\user or user@domain.tld handle both
            var sam = username.Contains('\\') ? username.Split('\\').Last() : username;
            try
            {
                // Attempt bind with supplied credentials against configured LDAP path.
                // DirectoryEntry will throw on invalid credentials.
                using var entry = new DirectoryEntry(ldapPath, username, password, AuthenticationTypes.Secure);
                // Force bind
                var native = entry.NativeObject;

                // Search for displayName of the logged in account
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
                // bind/search failed -> invalid credentials or connectivity issue
                return null;
            }
        }

        // Basic escaping for LDAP filter control characters
        private static string EscapeLdapSearchFilter(string input)
        {
            return input?.Replace("\\", "\\5c").Replace("*", "\\2a").Replace("(", "\\28").Replace(")", "\\29").Replace("\0", "\\00") ?? string.Empty;
        }

        public class CredentialInput
        {
            [Required]
            public string Username { get; set; }

            [Required]
            public string Password { get; set; }
        }
    }
}