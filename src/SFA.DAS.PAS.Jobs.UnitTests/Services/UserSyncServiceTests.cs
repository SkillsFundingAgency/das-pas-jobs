using AutoFixture.NUnit4;
using Moq;
using SFA.DAS.DfESignIn.Auth.Configuration;
using SFA.DAS.DfESignIn.Auth.Interfaces;
using SFA.DAS.PAS.Data.Entities;
using SFA.DAS.PAS.Data.Models;
using SFA.DAS.PAS.Data.Repositories;
using SFA.DAS.PAS.Jobs.ApiModels;
using SFA.DAS.PAS.Jobs.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.PAS.Jobs.UnitTests.Services;

[TestFixture]
public class UserSyncServiceTests
{
    [Test, MoqAutoData]
    public async Task WhenSyncUsers_AndNoProviderFound_ThenDoesNothing(
        [Frozen] Mock<IProviderRepository> providerRepository,
        [Frozen] Mock<IUserRepository> userRepository,
        [Frozen] Mock<IApiHelper> apiHelper,
        UserSyncService sut)
    {
        providerRepository
            .Setup(r => r.GetNextProviderForUpdate(It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        await sut.SyncUsers(CancellationToken.None);

        apiHelper.Verify(a => a.Get<GetDfeUsersResponse>(It.IsAny<string>()), Times.Never);
        userRepository.Verify(r => r.SyncUsers(It.IsAny<long>(), It.IsAny<List<UserModel>>(), It.IsAny<CancellationToken>()), Times.Never);
        providerRepository.Verify(r => r.MarkProviderUpdated(It.IsAny<Provider>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenSyncUsers_ThenGetsProviderUsersFromDfe(
        [Frozen] Mock<IProviderRepository> providerRepository,
        [Frozen] Mock<IApiHelper> apiHelper,
        [Frozen] DfEOidcConfiguration configuration,
        int ukprn,
        UserSyncService sut)
    {
        var provider = new Provider { Ukprn = ukprn, Name = "Test provider" };
        var expectedEndpoint = $"{configuration.APIServiceUrl}/organisations/{provider.Ukprn}/users";
        providerRepository
            .Setup(r => r.GetNextProviderForUpdate(It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);
        apiHelper
            .Setup(a => a.Get<GetDfeUsersResponse>(expectedEndpoint))
            .ReturnsAsync(new GetDfeUsersResponse { Users = [new DfeUser { Email = "a@test.com", UserStatus = 1 }] });

        await sut.SyncUsers(CancellationToken.None);

        apiHelper.Verify(a => a.Get<GetDfeUsersResponse>(expectedEndpoint), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenSyncUsers_ThenSyncsOnlyActiveUsersDistinctByEmail_AndMarksProviderUpdated(
        [Frozen] Mock<IProviderRepository> providerRepository,
        [Frozen] Mock<IUserRepository> userRepository,
        [Frozen] Mock<IApiHelper> apiHelper,
        int ukprn,
        UserSyncService sut,
        CancellationToken cancellationToken)
    {
        var provider = new Provider { Ukprn = ukprn, Name = "Test provider" };
        var response = new GetDfeUsersResponse
        {
            Users =
            [
                new DfeUser { Email = "Active@test.com", UserStatus = 1 },
                new DfeUser { Email = "active@TEST.com", UserStatus = 1 },
                new DfeUser { Email = "other@test.com", UserStatus = 1 },
                new DfeUser { Email = "inactive@test.com", UserStatus = 0 },
                new DfeUser { Email = "unknown@test.com", UserStatus = null },
            ],
        };
        List<UserModel> syncedUsers = [];
        providerRepository
            .Setup(r => r.GetNextProviderForUpdate(cancellationToken))
            .ReturnsAsync(provider);
        apiHelper
            .Setup(a => a.Get<GetDfeUsersResponse>(It.IsAny<string>()))
            .ReturnsAsync(response);
        userRepository
            .Setup(r => r.SyncUsers(provider.Ukprn, It.IsAny<List<UserModel>>(), cancellationToken))
            .Callback<long, List<UserModel>, CancellationToken>((_, users, _) => syncedUsers = users)
            .Returns(Task.CompletedTask);

        await sut.SyncUsers(cancellationToken);

        Assert.That(syncedUsers, Has.Count.EqualTo(2));
        Assert.That(syncedUsers.Select(u => u.Email), Is.EqualTo(new[] { "Active@test.com", "other@test.com" }));
        Assert.That(syncedUsers.All(u => u.UserType == UserType.NormalUser), Is.True);
        providerRepository.Verify(r => r.MarkProviderUpdated(provider, cancellationToken), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenSyncUsers_AndDfeResponseIsNull_ThenMarksProviderUpdatedWithoutSyncing(
        [Frozen] Mock<IProviderRepository> providerRepository,
        [Frozen] Mock<IUserRepository> userRepository,
        [Frozen] Mock<IApiHelper> apiHelper,
        int ukprn,
        UserSyncService sut,
        CancellationToken cancellationToken)
    {
        var provider = new Provider { Ukprn = ukprn, Name = "Test provider" };
        providerRepository
            .Setup(r => r.GetNextProviderForUpdate(cancellationToken))
            .ReturnsAsync(provider);
        apiHelper
            .Setup(a => a.Get<GetDfeUsersResponse>(It.IsAny<string>()))
            .ReturnsAsync((GetDfeUsersResponse?)null!);

        await sut.SyncUsers(cancellationToken);

        userRepository.Verify(
            r => r.SyncUsers(It.IsAny<long>(), It.IsAny<List<UserModel>>(), It.IsAny<CancellationToken>()),
            Times.Never);
        providerRepository.Verify(r => r.MarkProviderUpdated(provider, cancellationToken), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task WhenSyncUsers_AndNoActiveDfeUsers_ThenMarksProviderUpdatedWithoutSyncing(
        [Frozen] Mock<IProviderRepository> providerRepository,
        [Frozen] Mock<IUserRepository> userRepository,
        [Frozen] Mock<IApiHelper> apiHelper,
        int ukprn,
        UserSyncService sut,
        CancellationToken cancellationToken)
    {
        var provider = new Provider { Ukprn = ukprn, Name = "Test provider" };
        providerRepository
            .Setup(r => r.GetNextProviderForUpdate(cancellationToken))
            .ReturnsAsync(provider);
        apiHelper
            .Setup(a => a.Get<GetDfeUsersResponse>(It.IsAny<string>()))
            .ReturnsAsync(new GetDfeUsersResponse { Users = [new DfeUser { Email = "x@test.com", UserStatus = 0 }] });

        await sut.SyncUsers(cancellationToken);

        userRepository.Verify(
            r => r.SyncUsers(It.IsAny<long>(), It.IsAny<List<UserModel>>(), It.IsAny<CancellationToken>()),
            Times.Never);
        providerRepository.Verify(r => r.MarkProviderUpdated(provider, cancellationToken), Times.Once);
    }

    [Test, MoqAutoData]
    public void WhenSyncUsers_AndRepositoryThrows_ThenMarksProviderUpdated_AndRethrows(
        [Frozen] Mock<IProviderRepository> providerRepository,
        [Frozen] Mock<IUserRepository> userRepository,
        [Frozen] Mock<IApiHelper> apiHelper,
        int ukprn,
        UserSyncService sut,
        CancellationToken cancellationToken)
    {
        var provider = new Provider { Ukprn = ukprn, Name = "Test provider" };
        var exception = new InvalidOperationException("sync failed");
        providerRepository
            .Setup(r => r.GetNextProviderForUpdate(cancellationToken))
            .ReturnsAsync(provider);
        apiHelper
            .Setup(a => a.Get<GetDfeUsersResponse>(It.IsAny<string>()))
            .ReturnsAsync(new GetDfeUsersResponse { Users = [new DfeUser { Email = "a@test.com", UserStatus = 1 }] });
        userRepository
            .Setup(r => r.SyncUsers(It.IsAny<long>(), It.IsAny<List<UserModel>>(), cancellationToken))
            .Returns(Task.FromException(exception));

        var thrown = Assert.ThrowsAsync<InvalidOperationException>(() => sut.SyncUsers(cancellationToken));

        Assert.That(thrown, Is.SameAs(exception));
        providerRepository.Verify(r => r.MarkProviderUpdated(provider, cancellationToken), Times.Once);
    }
}
