using FNRC_DigitalHub.SharedResource_MarkerFile;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Reflection;

namespace FNRC_DigitalHub.Helper
{
    public class CommonLocalizationService
    {
        public IStringLocalizer localizer { get; }
        public CommonLocalizationService(IStringLocalizerFactory factory)
        {
            var assemblyName = new AssemblyName(typeof(SharedResource).GetTypeInfo().Assembly.FullName);
            localizer = factory.Create(nameof(SharedResource), assemblyName.Name);
        }
        public string Get(string key)
        {
            return localizer[key].ToString();
        }
        public string Get(string key, string language)
        {
            var culture = new CultureInfo(language);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            return localizer[key].ToString();
        }
        public HtmlString GetHTML(string key)
        {
            return new HtmlString(localizer[key].ToString());
        }
    }
}
