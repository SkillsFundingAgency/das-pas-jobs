using AutoFixture.NUnit4;
using Microsoft.Azure.Functions.Worker;
using Moq;
using SFA.DAS.PAS.Jobs.Functions;
using SFA.DAS.PAS.Jobs.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.PAS.Jobs.UnitTests.Functions;

[TestFixture]
public class SynchroniseProvidersFunctionTests
{
    [Test, MoqAutoData]
    public async Task Run_CallsImportProvidersWithCancellationToken(
        [Frozen] Mock<IImportProviderService> importProviderService,
        TimerInfo timerInfo,
        CancellationToken cancellationToken,
        SynchroniseProvidersFunction function)
    {
        await function.Run(timerInfo, cancellationToken);

        importProviderService.Verify(s => s.ImportProviders(cancellationToken), Times.Once);
    }

    [Test, MoqAutoData]
    public void Run_WhenServiceThrows_ExceptionPropagates(
        [Frozen] Mock<IImportProviderService> importProviderService,
        TimerInfo timerInfo,
        CancellationToken cancellationToken,
        SynchroniseProvidersFunction function)
    {
        var exception = new InvalidOperationException("failed");
        importProviderService.Setup(s => s.ImportProviders(cancellationToken)).Returns(Task.FromException(exception));

        var thrown = Assert.ThrowsAsync<InvalidOperationException>(() => function.Run(timerInfo, cancellationToken));

        Assert.That(thrown, Is.SameAs(exception));
    }
}
