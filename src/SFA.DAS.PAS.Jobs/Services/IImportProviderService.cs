using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.PAS.Jobs.Services;

public interface IImportProviderService
{
    Task ImportProviders(CancellationToken cancellationToken);
}
