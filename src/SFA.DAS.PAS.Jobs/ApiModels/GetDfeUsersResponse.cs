using System.Collections.Generic;
using Newtonsoft.Json;

namespace SFA.DAS.PAS.Jobs.ApiModels;


public class GetDfeUsersResponse
{
    [JsonProperty("ukprn")]
    public string Ukprn { get; set; }

    [JsonProperty("users")]
    public List<DfeUser> Users { get; set; }
}

public class DfeUser
{
    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("firstName")]
    public string FirstName { get; set; }

    [JsonProperty("lastName")]
    public string LastName { get; set; }

    [JsonProperty("userStatus", NullValueHandling = NullValueHandling.Ignore)]
    public long? UserStatus { get; set; }

    [JsonProperty("roles")]
    public List<string> Roles { get; set; }
}