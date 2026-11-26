namespace SFA.DAS.PAS.Data.Entities;

public class UserSettings
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public required string UserRef { get; set; }
    public bool ReceiveNotifications { get; set; }
    public virtual User User { get; set; } = null!;
}
