using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices; 
using DirectoryEntry = System.DirectoryServices.DirectoryEntry;

namespace FNRC_DigitalHub.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WhoAmIController(IConfiguration config) : ControllerBase
    {
        public IConfiguration Config { get; } = config;

        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
             
            var identity = User?.Identity;
            if (identity == null || !identity.IsAuthenticated)
                return Unauthorized();
             
            var domainUser = identity.Name;
            var parts = domainUser.Split('\\');
            string samAccountName = parts.Length == 2 ? parts[1] : domainUser;

            var displayName = GetDisplayNameFromAd(samAccountName);
            return Ok(new { UserName = samAccountName, DisplayName = displayName });
        }

        private string GetDisplayNameFromAd(string samAccountName)
        {
             
            string ldapPath = Config["LDAP:Domain"].ToString();
            using var entry = new DirectoryEntry(ldapPath);
            using var searcher = new DirectorySearcher(entry)
            {
                Filter = $"(sAMAccountName={EscapeLdapSearchFilter(samAccountName)})"
            };
            searcher.PropertiesToLoad.Add("displayName");
            var res = searcher.FindOne();
            if (res != null && res.Properties["displayName"].Count > 0)
            {
                return res.Properties["displayName"][0].ToString();
            }
            return samAccountName;  
        }

        // Basic escaping for LDAP filter control characters
        private string EscapeLdapSearchFilter(string input)
        {
            return input.Replace("\\", "\\5c").Replace("*", "\\2a").Replace("(", "\\28").Replace(")", "\\29").Replace("\0", "\\00");
        }
    }
}
