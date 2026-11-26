using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.DfESignIn.Auth.Configuration;
using SFA.DAS.DfESignIn.Auth.Interfaces;
using SFA.DAS.PAS.Data.Entities;
using SFA.DAS.PAS.Data.Models;
using SFA.DAS.PAS.Data.Repositories;
using SFA.DAS.PAS.Jobs.ApiModels;

namespace SFA.DAS.PAS.Jobs.Services;

public class UserSyncService(
    IUserRepository userRepository,
    IProviderRepository providerRepository,
    ILogger<UserSyncService> logger,
    IApiHelper apiHelper,
    DfEOidcConfiguration dfEOidcConfiguration) : IUserSyncService
{
    public async Task SyncUsers(CancellationToken cancellationToken)
    {
        var provider = await providerRepository.GetNextProviderForUpdate(cancellationToken);

        if (provider == null)
        {
            logger.LogInformation("SyncUsers - No Provider Found");
            return;
        }

        logger.LogInformation("SyncUsers For Provider {Ukprn} has started", provider.Ukprn);

        var dfeUsers = await GetUsersFromDfe(provider.Ukprn);

        if (dfeUsers.Count == 0)
        {
            var emptyResponseMessage = $"There was an issue finding users for Provider {provider.Ukprn}. The request was not successful or returned no users.";
            await LogAndUpdateProviderState(new InvalidOperationException(emptyResponseMessage), provider, emptyResponseMessage, cancellationToken);
            return;
        }

        try
        {
            await userRepository.SyncUsers(provider.Ukprn, dfeUsers, cancellationToken);
            await providerRepository.MarkProviderUpdated(provider, cancellationToken);
            logger.LogInformation("Synchronise Users completed for Provider {Ukprn}", provider.Ukprn);
        }
        catch (Exception ex)
        {
            var message = $"An error occurred retrieving users from Provider {provider.Ukprn}";
            await LogAndUpdateProviderState(ex, provider, message, cancellationToken);
            throw;
        }
    }

    private Task LogAndUpdateProviderState(Exception ex, Provider provider, string errorMessage, CancellationToken cancellationToken)
    {
        logger.LogWarning(ex, "{ErrorMessage}", errorMessage);
        return providerRepository.MarkProviderUpdated(provider, cancellationToken);
    }

    private async Task<List<UserModel>> GetUsersFromDfe(long providerId)
    {
        var endpoint = $"{dfEOidcConfiguration.APIServiceUrl}/organisations/{providerId}/users";
        var response = await apiHelper.Get<GetDfeUsersResponse>(endpoint);

        if (response?.Users == null)
        {
            logger.LogInformation("{Endpoint} - None found", endpoint);
            return [];
        }

        logger.LogInformation("{Endpoint} - Found {Count}", endpoint, response.Users.Count);

        return response.Users
            .Where(u => u.UserStatus == 1)
            .GroupBy(u => u.Email, StringComparer.OrdinalIgnoreCase)
            .Select(g => new UserModel { Email = g.Key, UserType = UserType.NormalUser })
            .ToList();
    }
}
