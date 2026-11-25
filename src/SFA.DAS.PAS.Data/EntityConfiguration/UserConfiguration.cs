using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.PAS.Data.Entities;

namespace SFA.DAS.PAS.Data.EntityConfiguration;

[ExcludeFromCodeCoverage]
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User", "dbo");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Ukprn)
            .HasConversion<long>();

        builder.HasOne(user => user.UserSettings)
            .WithOne(settings => settings.User)
            .HasForeignKey<UserSettings>(settings => settings.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
