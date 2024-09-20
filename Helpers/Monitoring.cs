using System.Diagnostics;
using System.Reflection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Enrichers.Span;

namespace Helpers;

public static class Monitoring
{
    public static readonly ActivitySource ActivitySource = new("RPS", "1.0.0");
    public static TracerProvider TracerProvider;

    static Monitoring()
    {
        // Configure tracing
        var serviceName = Assembly.GetExecutingAssembly().GetName().Name;
        var version = "1.0.0";

        TracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddZipkinExporter()
            .AddConsoleExporter()
            .AddSource(ActivitySource.Name)
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: serviceName, serviceVersion: version))
            .Build();
        
        // Configure logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.WithSpan()
            .WriteTo.Seq("http://localhost:5341")
            .WriteTo.Console()
            .CreateLogger();
    }
}