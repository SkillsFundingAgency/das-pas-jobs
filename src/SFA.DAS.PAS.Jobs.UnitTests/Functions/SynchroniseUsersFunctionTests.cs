using AutoFixture.NUnit4;
using Microsoft.Azure.Functions.Worker;
using Moq;
using SFA.DAS.PAS.Jobs.Functions;
using SFA.DAS.PAS.Jobs.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.PAS.Jobs.UnitTests.Functions;

[TestFixture]
public class SynchroniseUsersFunctionTests
{
    [Test, MoqAutoData]
    public async Task Run_CallsSyncUsersWithCancellationToken(
        [Frozen] Mock<IUserSyncService> userSyncService,
        TimerInfo timerInfo,
        CancellationToken cancellationToken,
        SynchroniseUsersFunction function)
    {
        await function.Run(timerInfo, cancellationToken);

        userSyncService.Verify(s => s.SyncUsers(cancellationToken), Times.Once);
    }

    [Test, MoqAutoData]
    public void Run_WhenServiceThrows_ExceptionPropagates(
        [Frozen] Mock<IUserSyncService> userSyncService,
        TimerInfo timerInfo,
        CancellationToken cancellationToken,
        SynchroniseUsersFunction function)
    {
        var exception = new InvalidOperationException("failed");
        userSyncService.Setup(s => s.SyncUsers(cancellationToken)).Returns(Task.FromException(exception));

        var thrown = Assert.ThrowsAsync<InvalidOperationException>(() => function.Run(timerInfo, cancellationToken));

        Assert.That(thrown, Is.SameAs(exception));
    }
}
