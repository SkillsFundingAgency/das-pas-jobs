namespace SFA.DAS.PAS.Data.Entities;

public class Provider
{
    public int Ukprn { get; set; }
    public required string Name { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }
    public DateTime? UpdatedFromIDAMS { get; set; }
    public virtual List<User> Users { get; set; } = [];
}
