namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.WorkFlows;

using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class InterviewAttendeeConfiguration : IEntityTypeConfiguration<InterviewAttendee>
{
    public void Configure(EntityTypeBuilder<InterviewAttendee> builder)
    {
        builder.ToTable("WorkFlows_Actions_InterviewAttendees");

        builder
            .HasKey(attendee => attendee.Id);

        builder
            .Property(attendee => attendee.Id)
            .HasConversion(
                id => id.Value,
                value => InterviewAttendeeId.FromValue(value));
    }
}