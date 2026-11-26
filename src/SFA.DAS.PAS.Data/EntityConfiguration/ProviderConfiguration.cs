using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.PAS.Data.Entities;

namespace SFA.DAS.PAS.Data.EntityConfiguration;

[ExcludeFromCodeCoverage]
public class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.HasKey(provider => provider.Ukprn);

        builder.Property(provider => provider.Ukprn)
            .HasConversion<long>();

        builder.HasMany(provider => provider.Users)
            .WithOne(user => user.Provider)
            .HasForeignKey(user => user.Ukprn)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
