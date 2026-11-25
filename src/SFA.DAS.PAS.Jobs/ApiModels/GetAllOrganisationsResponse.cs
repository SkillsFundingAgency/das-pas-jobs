using System.Collections.Generic;

namespace SFA.DAS.PAS.Jobs.ApiModels;

public class GetAllOrganisationsResponse
{
    public List<Organisation> Organisations { get; set; } = [];
}
