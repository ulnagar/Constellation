namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Hosting;

using Constellation.Core.Models.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class NewsletterConfiguration : IEntityTypeConfiguration<Newsletter>
{
    public void Configure(EntityTypeBuilder<Newsletter> builder)
    {
        builder.ToTable("Newsletters", "Hosting");

        builder
            .HasKey(newsletter => newsletter.Issue);

        builder
            .Property(newsletter => newsletter.Issue)
            .ValueGeneratedNever();
    }
}
