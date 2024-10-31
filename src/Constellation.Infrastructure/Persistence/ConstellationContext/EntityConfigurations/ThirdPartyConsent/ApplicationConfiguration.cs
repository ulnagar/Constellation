namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.ThirdPartyConsent;

using Core.Models.ThirdPartyConsent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApplicationId = Core.Models.ThirdPartyConsent.Identifiers.ApplicationId;

internal sealed class ApplicationConfiguration : IEntityTypeConfiguration<Application>
{
    public void Configure(EntityTypeBuilder<Application> builder)
    {
        builder.ToTable("Applications", "ThirdParty");

        builder
            .HasKey(application => application.Id);

        builder
            .Property(application => application.Id)
            .HasConversion(
                id => id.Value,
                value => ApplicationId.FromValue(value));
        
        builder
            .Navigation(application => application.Consents)
            .AutoInclude();
        
        builder
            .Navigation(application => application.Requirements)
            .AutoInclude();

        builder
            .Property(application => application.InformationCollected)
            .HasConversion(
                list => string.Join('|', list),
                value => value.Split('|', StringSplitOptions.RemoveEmptyEntries));

        builder
            .Property(application => application.SharedWith)
            .HasConversion(
                list => string.Join('|', list),
                value => value.Split('|', StringSplitOptions.RemoveEmptyEntries));
    }
}