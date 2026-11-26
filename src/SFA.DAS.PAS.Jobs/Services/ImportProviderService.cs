using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.PAS.Data.Entities;
using SFA.DAS.PAS.Data.Repositories;
using SFA.DAS.PAS.Jobs.ApiClients;

namespace SFA.DAS.PAS.Jobs.Services;

public class ImportProviderService(
    IRoatpApiClient roatpApiClient,
    IProviderRepository providerRepository,
    ILogger<ImportProviderService> logger) : IImportProviderService
{
    private const int BatchSize = 1000;

    public async Task ImportProviders(CancellationToken cancellationToken)
    {
        logger.LogInformation("Import Providers Service - Started");

        ApiModels.GetAllOrganisationsResponse providersResponse = await roatpApiClient.GetProviders();
        if (providersResponse == null || providersResponse.Organisations.Count == 0)
        {
            logger.LogInformation("ImportProvidersJob - No providers returned");
            return;
        }

        foreach (var batch in providersResponse.Organisations.Chunk(BatchSize))
        {
            var mappedProviders = batch
                .Select(provider => new Provider
                {
                    Ukprn = provider.Ukprn,
                    Name = provider.LegalName,
                })
                .ToArray();

            await providerRepository.ImportProviders(mappedProviders, cancellationToken);
        }

        logger.LogInformation("Import Providers Service - Finished");
    }
}
