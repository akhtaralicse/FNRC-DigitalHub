using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;

public static class SerilogLogger
{
    public static void ConfigureLogging(IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .WriteTo.Seq("http://localhost:5341")
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Project", "FNRC_DigitalHub-App")
            .Enrich.WithMachineName()
            .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
            .WithDefaultDestructurers())
            .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
            .Enrich.WithProperty("InnovationLogs", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
#if DEBUG
            .Enrich.WithProperty("DebuggerAttached", System.Diagnostics.Debugger.IsAttached)
#endif
            .CreateLogger();

        Log.Information("===Serilog is configured and running.====");
    }

    public static IHostBuilder UseSerilogLogging(this IHostBuilder builder, IConfiguration configuration)
    {
        ConfigureLogging(configuration);

        builder.UseSerilog();
        return builder;
    }
}
