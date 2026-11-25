namespace SFA.DAS.PAS.Data.Entities;

public class User
{
    public long Id { get; set; }
    public required string UserRef { get; set; }
    public required string DisplayName { get; set; }
    public required int Ukprn { get; set; }
    public required string Email { get; set; }
    public bool IsDeleted { get; set; }
    public UserType UserType { get; set; }
    public DateTime? LastLogin { get; set; }
    public virtual UserSettings? UserSettings { get; set; }
    public required virtual Provider Provider { get; set; }
}
