using Constellation.Infrastructure.Persistence.TrackItContext.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.TrackItContext.EntityConfigurations
{
    public class LocationConfiguration : IEntityTypeConfiguration<Location>
    {
        public void Configure(EntityTypeBuilder<Location> entity)
        {
            entity.HasKey(e => e.Sequence).HasName("PK_LOCATION_");

            entity.ToTable("_LOCATION_", "_SMDBA_", tb =>
            {
                tb.HasTrigger("sm_ad__LOCATION_st");
                tb.HasTrigger("sm_ai__LOCATION_st");
                tb.HasTrigger("sm_au__LOCATION_st");
            });

            entity.HasIndex(e => new { e.Country, e.Sequence }, "Country");

            entity.HasIndex(e => new { e.SeqCountry, e.Sequence }, "FK_LOCATION_SEQ_COUNTRY");

            entity.HasIndex(e => e.SeqPriority, "FK_LOCATION_SEQ_PRIORITY");

            entity.HasIndex(e => new { e.Group, e.Sequence }, "FK_LOCATION__GROUP_");

            entity.HasIndex(e => new { e.Owner, e.Sequence }, "FK_LOCATION__OWNER_");

            entity.HasIndex(e => e.Name, "UQ_LOCATION_NAME").IsUnique();

            entity.Property(e => e.Sequence)
                .ValueGeneratedNever()
                .HasColumnName("SEQUENCE");
            entity.Property(e => e.Address)
                .HasMaxLength(128)
                .HasColumnName("ADDRESS");
            entity.Property(e => e.City)
                .HasMaxLength(20)
                .HasColumnName("CITY");
            entity.Property(e => e.Code)
                .HasMaxLength(15)
                .HasColumnName("CODE");
            entity.Property(e => e.Comments)
                .HasMaxLength(255)
                .HasColumnName("COMMENTS");
            entity.Property(e => e.Country)
                .HasMaxLength(25)
                .HasColumnName("COUNTRY");
            entity.Property(e => e.Fax)
                .HasMaxLength(13)
                .HasColumnName("FAX");
            entity.Property(e => e.Group).HasColumnName("_GROUP_");
            entity.Property(e => e.Inactive).HasColumnName("_INACTIVE_:");
            entity.Property(e => e.IntlFax)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("INTL_FAX_#");
            entity.Property(e => e.IntlPhone)
                .HasMaxLength(30)
                .IsFixedLength()
                .HasColumnName("INTL_PHONE_#");
            entity.Property(e => e.IntlPostCode)
                .HasMaxLength(9)
                .IsFixedLength()
                .HasColumnName("INTL_POST_CODE");
            entity.Property(e => e.Lastmodified)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("LASTMODIFIED");
            entity.Property(e => e.Lastuser)
                .HasMaxLength(255)
                .HasDefaultValueSql("(user_name())")
                .HasColumnName("LASTUSER");
            entity.Property(e => e.MainContact)
                .HasMaxLength(50)
                .HasColumnName("MAIN_CONTACT");
            entity.Property(e => e.Maincontctphone)
                .HasMaxLength(30)
                .HasColumnName("MAINCONTCTPHONE");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("NAME");
            entity.Property(e => e.Note).HasColumnName("NOTE");
            entity.Property(e => e.Owner).HasColumnName("_OWNER_");
            entity.Property(e => e.Ownerperms).HasColumnName("_OWNERPERMS_");
            entity.Property(e => e.Phone)
                .HasMaxLength(30)
                .HasColumnName("PHONE");
            entity.Property(e => e.SeqCountry).HasColumnName("SEQ_COUNTRY");
            entity.Property(e => e.SeqPriority).HasColumnName("SEQ_PRIORITY");
            entity.Property(e => e.State)
                .HasMaxLength(25)
                .HasColumnName("STATE");
            entity.Property(e => e.TimeZone)
                .HasMaxLength(10)
                .HasColumnName("TIME_ZONE");
            entity.Property(e => e.Zip)
                .HasMaxLength(10)
                .HasColumnName("ZIP");
        }
    }

}
