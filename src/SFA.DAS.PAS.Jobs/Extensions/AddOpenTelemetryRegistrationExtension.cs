using System.Diagnostics.CodeAnalysis;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SFA.DAS.PAS.Api.AppStart;

[ExcludeFromCodeCoverage]
public static class AddOpenTelemetryRegistrationExtension
{
    public static IServiceCollection AddOpenTelemetryRegistration(this IServiceCollection services, IConfiguration configuration)
    {
        var appInsightsConnectionString = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
        if (!string.IsNullOrEmpty(appInsightsConnectionString))
        {
            // This service will collect and send telemetry data to Azure Monitor.
            services.AddOpenTelemetry().UseAzureMonitor(options =>
            {
                options.ConnectionString = appInsightsConnectionString;
            });
        }

        return services;
    }
}
