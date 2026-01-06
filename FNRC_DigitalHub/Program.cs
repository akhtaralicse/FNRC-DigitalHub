using DigitalHub.Domain.DBContext;
using DigitalHub.Services.Shared;
using FNRC_DigitalHub.Helper;
using FNRC_DigitalHub.Middleware;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

string xLoginPath = "/Account/Login";
ConfigurationManager configuration = builder.Configuration;
ConfigureHostBuilder hostBuilder = builder.Host;
var connectionDB = builder.Configuration.GetConnectionString("SqlConnectionStrings");
builder.Services.AddDbContext<DigitalHubDBContext>(options => options.UseSqlServer(connectionDB
    //,    options => options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
    )
);

builder.Services.AddSingleton<CommonLocalizationService>();
builder.Services.AddMvc().AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);
//builder.Services.AddSingleton<SharedResource>();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(int.Parse(configuration["Session:ExpireDuration"]));
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.PropertyNamingPolicy = null; // Keeps PascalCase
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = xLoginPath;
    options.Cookie.Name = "DSINTRANET";
    options.Cookie.IsEssential = true;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = xLoginPath;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(int.Parse(configuration["Session:ExpireDuration"]));
});
builder.Services.CustomServicesBuilder(configuration);

var serilogConfig = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("serilog.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();
builder.Host.UseSerilogLogging(serilogConfig);
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "ar", "en" };
    options.SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

var app = builder.Build();

app.UseRequestLocalization();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    
    app.UseHsts();
}
app.UseRouting();
app.UseAuthorization();
app.UseSession();

//app.UseCors(builder =>
//    builder.WithOrigins("http://localhost:7280", "https://localhost:3000")
//           .AllowCredentials()
//           .AllowAnyMethod()
//           .AllowAnyHeader());

app.UseMiddleware<SessionTimeoutMiddleware>();
app.UseMiddleware<GlobalErrorHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
 

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
