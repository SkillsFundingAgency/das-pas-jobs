using AutoFixture.NUnit4;
using Moq;
using SFA.DAS.PAS.Data.Entities;
using SFA.DAS.PAS.Data.Repositories;
using SFA.DAS.PAS.Jobs.ApiClients;
using SFA.DAS.PAS.Jobs.ApiModels;
using SFA.DAS.PAS.Jobs.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.PAS.Jobs.UnitTests.Services;

[TestFixture]
public class ImportProviderServiceTests
{
    [Test, MoqAutoData]
    public async Task WhenImportProviders_ThenMapsProviders_AndPassesCancellationToken(
        [Frozen] Mock<IRoatpApiClient> roatpApiClient,
        [Frozen] Mock<IProviderRepository> providerRepository,
        ImportProviderService sut,
        CancellationToken cancellationToken)
    {
        var response = new GetAllOrganisationsResponse
        {
            Organisations =
            [
                new Organisation { Ukprn = 10000001, LegalName = "Provider One" },
                new Organisation { Ukprn = 10000002, LegalName = "Provider Two" },
            ],
        };
        Provider[]? importedProviders = null;
        CancellationToken? importedCancellationToken = null;
        roatpApiClient.Setup(client => client.GetProviders()).ReturnsAsync(response);
        providerRepository
            .Setup(repository => repository.ImportProviders(It.IsAny<Provider[]>(), It.IsAny<CancellationToken>()))
            .Callback<Provider[], CancellationToken>((providers, token) =>
            {
                importedProviders = providers;
                importedCancellationToken = token;
            })
            .Returns(Task.CompletedTask);

        await sut.ImportProviders(cancellationToken);

        roatpApiClient.Verify(client => client.GetProviders(), Times.Once);
        Assert.That(importedProviders, Has.Length.EqualTo(2));
        Assert.That(importedProviders![0].Ukprn, Is.EqualTo(10000001));
        Assert.That(importedProviders[0].Name, Is.EqualTo("Provider One"));
        Assert.That(importedProviders[1].Ukprn, Is.EqualTo(10000002));
        Assert.That(importedProviders[1].Name, Is.EqualTo("Provider Two"));
        Assert.That(importedCancellationToken, Is.EqualTo(cancellationToken));
    }

    [Test, MoqAutoData]
    public async Task WhenImportProviders_ThenDoesNotImportWhenApiReturnsNoProviders(
        [Frozen] Mock<IRoatpApiClient> roatpApiClient,
        [Frozen] Mock<IProviderRepository> providerRepository,
        ImportProviderService sut)
    {
        roatpApiClient
            .Setup(client => client.GetProviders())
            .ReturnsAsync(new GetAllOrganisationsResponse());

        await sut.ImportProviders(CancellationToken.None);

        roatpApiClient.Verify(client => client.GetProviders(), Times.Once);
        providerRepository.Verify(
            repository => repository.ImportProviders(It.IsAny<Provider[]>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test, MoqAutoData]
    public async Task WhenImportProviders_ThenImportsProvidersInBatchesOfAtMostOneThousand(
        [Frozen] Mock<IRoatpApiClient> roatpApiClient,
        [Frozen] Mock<IProviderRepository> providerRepository,
        ImportProviderService sut,
        CancellationToken cancellationToken)
    {
        var response = new GetAllOrganisationsResponse
        {
            Organisations = Enumerable.Range(1, 1001)
                .Select(index => new Organisation { Ukprn = index, LegalName = $"Provider {index}" })
                .ToList(),
        };
        var importedBatches = new List<Provider[]>();
        roatpApiClient.Setup(client => client.GetProviders()).ReturnsAsync(response);
        providerRepository
            .Setup(repository => repository.ImportProviders(It.IsAny<Provider[]>(), It.IsAny<CancellationToken>()))
            .Callback<Provider[], CancellationToken>((providers, _) => importedBatches.Add(providers))
            .Returns(Task.CompletedTask);

        await sut.ImportProviders(cancellationToken);

        Assert.That(importedBatches, Has.Count.EqualTo(2));
        Assert.That(importedBatches[0], Has.Length.EqualTo(1000));
        Assert.That(importedBatches[1], Has.Length.EqualTo(1));
        Assert.That(importedBatches[0][0].Ukprn, Is.EqualTo(1));
        Assert.That(importedBatches[0][^1].Ukprn, Is.EqualTo(1000));
        Assert.That(importedBatches[1][0].Ukprn, Is.EqualTo(1001));
    }
}
