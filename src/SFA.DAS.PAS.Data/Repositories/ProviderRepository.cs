using System.Data;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.PAS.Data.Entities;

namespace SFA.DAS.PAS.Data.Repositories;

public interface IProviderRepository
{
    Task ImportProviders(Provider[] providers, CancellationToken cancellationToken);
    Task<Provider?> GetNextProviderForUpdate(CancellationToken cancellationToken = default);
    Task MarkProviderUpdated(Provider provider, CancellationToken cancellationToken);
}

[ExcludeFromCodeCoverage]
internal class ProviderRepository(IPasDataContext _pasDataContext) : IProviderRepository
{
    public async Task ImportProviders(Provider[] providers, CancellationToken cancellationToken)
    {
        var providersDataTable = new DataTable();
        providersDataTable.Columns.Add("Ukprn");
        providersDataTable.Columns.Add("Name");

        foreach (var provider in providers)
        {
            providersDataTable.Rows.Add(provider.Ukprn, provider.Name);
        }

        var parameters = new[]
        {
            new SqlParameter("@providers", SqlDbType.Structured)
            {
                TypeName = "dbo.Providers",
                Value = providersDataTable,
            },
            new SqlParameter("@now", SqlDbType.DateTime2) { Value = DateTime.UtcNow },
        };

        await _pasDataContext.Database.ExecuteSqlRawAsync(
            "EXEC [dbo].[ImportProviders] @providers, @now",
            parameters,
            cancellationToken);
    }

    public Task<Provider?> GetNextProviderForUpdate(CancellationToken cancellationToken = default)
    {
        return _pasDataContext.Providers
            .Where(provider => provider.Users.Any())
            .OrderBy(provider => provider.UpdatedFromIDAMS)
            .ThenBy(provider => provider.Ukprn)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task MarkProviderUpdated(Provider provider, CancellationToken cancellationToken)
    {
        provider.UpdatedFromIDAMS = DateTime.UtcNow;
        await _pasDataContext.SaveChangesAsync(cancellationToken);
    }
}
