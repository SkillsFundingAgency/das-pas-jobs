using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.PAS.Data.Repositories;

namespace SFA.DAS.PAS.Data.Extensions;

[ExcludeFromCodeCoverage]
public static class AddPasDataContextExtension
{
    public static IServiceCollection AddPasDataContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PasDataContext>((serviceProvider, options) =>
        {
            var connectionString = configuration["DatabaseConnectionString"];
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("DatabaseConnectionString is not configured");
            }
            var connection = new SqlConnection(connectionString);

            options.UseSqlServer(connection, o => o.EnableRetryOnFailure(5, TimeSpan.FromSeconds(20), null));
        });

        services
            .AddHealthChecks()
            .AddDbContextCheck<PasDataContext>();

        services.AddTransient<IPasDataContext, PasDataContext>(provider => provider.GetService<PasDataContext>()!);

        AddRepositories(services);

        return services;
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IProviderRepository, ProviderRepository>();
    }
}
