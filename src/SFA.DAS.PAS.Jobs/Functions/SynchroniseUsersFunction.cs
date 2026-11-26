using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.PAS.Jobs.Services;

namespace SFA.DAS.PAS.Jobs.Functions;

public class SynchroniseUsersFunction(IUserSyncService userSyncService, ILogger<SynchroniseUsersFunction> logger)
{
    [Function(nameof(SynchroniseUsersFunction))]
    public async Task Run([TimerTrigger("%SynchroniseUsersFunctionSchedule%", RunOnStartup = true)] TimerInfo timerInfo, CancellationToken cancellationToken)
    {
        logger.LogInformation("SynchroniseUsersFunction started");

        await userSyncService.SyncUsers(cancellationToken);

        logger.LogInformation("SynchroniseUsersFunction completed");
    }
}
