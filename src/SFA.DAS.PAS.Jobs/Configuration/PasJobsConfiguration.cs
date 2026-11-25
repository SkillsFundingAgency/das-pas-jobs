using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.PAS.Jobs.Configuration;

[ExcludeFromCodeCoverage]
public class PasJobsConfiguration
{
    public string DatabaseConnectionString { get; set; }
    public RoatpApiConfiguration RoatpApiClient { get; set; }
}
