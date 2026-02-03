using Constellation.Infrastructure.Persistence.TrackItContext.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.TrackItContext.EntityConfigurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> entity)
        {
            entity.HasKey(e => e.Sequence).HasName("PK_CUSTOMER_");

            entity.ToTable("_CUSTOMER_", "_SMDBA_", tb =>
            {
                tb.HasTrigger("sm_ad__CUSTOMER_st");
                tb.HasTrigger("sm_ai__CUSTOMER_st");
                tb.HasTrigger("sm_au__CUSTOMER_st");
            });

            entity.HasIndex(e => e.Winuserid, "CUSTOMER_WINUSERID");

            entity.HasIndex(e => new { e.Fname, e.Sequence }, "Client First Name");

            entity.HasIndex(e => new { e.Name, e.Sequence }, "Client Last Name");

            entity.HasIndex(e => new { e.BillTo, e.Sequence }, "FK_CUSTOMER_BILL TO");

            entity.HasIndex(e => new { e.Dept, e.Sequence }, "FK_CUSTOMER_DEPT");

            entity.HasIndex(e => new { e.Location, e.Sequence }, "FK_CUSTOMER_LOCATION");

            entity.HasIndex(e => new { e.SeqCountry, e.Sequence }, "FK_CUSTOMER_SEQ_COUNTRY");

            entity.HasIndex(e => e.SeqPriority, "FK_CUSTOMER_SEQ_PRIORITY");

            entity.HasIndex(e => new { e.SeqStaff, e.Sequence }, "FK_CUSTOMER_SEQ_STAFF");

            entity.HasIndex(e => new { e.Group, e.Sequence }, "FK_CUSTOMER__GROUP_");

            entity.HasIndex(e => new { e.Owner, e.Sequence }, "FK_CUSTOMER__OWNER_");

            entity.HasIndex(e => e.Client, "UQ_CUSTOMER_CLIENT").IsUnique();

            entity.Property(e => e.Sequence)
                .ValueGeneratedNever()
                .HasColumnName("SEQUENCE");
            entity.Property(e => e.Address)
                .HasMaxLength(128)
                .HasColumnName("ADDRESS");
            entity.Property(e => e.BillTo).HasColumnName("BILL TO");
            entity.Property(e => e.Bldng)
                .HasMaxLength(10)
                .HasColumnName("BLDNG");
            entity.Property(e => e.City)
                .HasMaxLength(20)
                .HasColumnName("CITY");
            entity.Property(e => e.CliCcdt01)
                .HasColumnType("datetime")
                .HasColumnName("CLI_CCDT01");
            entity.Property(e => e.CliCcdt02)
                .HasColumnType("datetime")
                .HasColumnName("CLI_CCDT02");
            entity.Property(e => e.CliCcint01).HasColumnName("CLI_CCINT01");
            entity.Property(e => e.CliCcint02).HasColumnName("CLI_CCINT02");
            entity.Property(e => e.CliCctxt01)
                .HasMaxLength(80)
                .HasColumnName("CLI_CCTXT01");
            entity.Property(e => e.CliCctxt02)
                .HasMaxLength(80)
                .HasColumnName("CLI_CCTXT02");
            entity.Property(e => e.CliCctxt03)
                .HasMaxLength(80)
                .HasColumnName("CLI_CCTXT03");
            entity.Property(e => e.CliCctxt04)
                .HasMaxLength(80)
                .HasColumnName("CLI_CCTXT04");
            entity.Property(e => e.CliCctxt05)
                .HasMaxLength(80)
                .HasColumnName("CLI_CCTXT05");
            entity.Property(e => e.CliCctxt06)
                .HasMaxLength(80)
                .HasColumnName("CLI_CCTXT06");
            entity.Property(e => e.Client)
                .HasMaxLength(255)
                .HasColumnName("CLIENT");
            entity.Property(e => e.Country)
                .HasMaxLength(25)
                .HasColumnName("COUNTRY");
            entity.Property(e => e.Createdfromssd)
                .HasDefaultValue((short)0)
                .HasColumnName("CREATEDFROMSSD:");
            entity.Property(e => e.Dept).HasColumnName("DEPT");
            entity.Property(e => e.Displayclientcomments)
                .HasDefaultValue((short)0)
                .HasColumnName("DISPLAYCLIENTCOMMENTS:");
            entity.Property(e => e.DoNotSurvey).HasColumnName("DO_NOT_SURVEY");
            entity.Property(e => e.Emailid)
                .HasMaxLength(1024)
                .HasColumnName("_EMAILID_");
            entity.Property(e => e.Ext)
                .HasMaxLength(5)
                .HasColumnName("EXT");
            entity.Property(e => e.Fax)
                .HasMaxLength(30)
                .HasColumnName("FAX");
            entity.Property(e => e.Fname)
                .HasMaxLength(50)
                .HasColumnName("FNAME");
            entity.Property(e => e.Group).HasColumnName("_GROUP_");
            entity.Property(e => e.Inactive).HasColumnName("_INACTIVE_:");
            entity.Property(e => e.LastSurveyed)
                .HasColumnType("datetime")
                .HasColumnName("LAST_SURVEYED");
            entity.Property(e => e.Lastmodified)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("LASTMODIFIED");
            entity.Property(e => e.Lastuser)
                .HasMaxLength(255)
                .HasDefaultValueSql("(user_name())")
                .HasColumnName("LASTUSER");
            entity.Property(e => e.Location).HasColumnName("LOCATION");
            entity.Property(e => e.Logininfo)
                .HasMaxLength(50)
                .HasColumnName("LOGININFO");
            entity.Property(e => e.Mail)
                .HasMaxLength(10)
                .HasColumnName("MAIL");
            entity.Property(e => e.Mname)
                .HasMaxLength(1)
                .IsFixedLength()
                .HasColumnName("MNAME");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("NAME");
            entity.Property(e => e.Note).HasColumnName("NOTE");
            entity.Property(e => e.NumSurveys).HasColumnName("NUM_SURVEYS");
            entity.Property(e => e.Owner).HasColumnName("_OWNER_");
            entity.Property(e => e.Ownerperms).HasColumnName("_OWNERPERMS_");
            entity.Property(e => e.Phone)
                .HasMaxLength(30)
                .HasColumnName("PHONE");
            entity.Property(e => e.Picture)
                .HasMaxLength(13)
                .HasColumnName("PICTURE");
            entity.Property(e => e.Position)
                .HasMaxLength(100)
                .HasColumnName("POSITION");
            entity.Property(e => e.Salt)
                .HasMaxLength(512)
                .HasColumnName("SALT");
            entity.Property(e => e.Selfserviceaccess)
                .HasMaxLength(80)
                .HasColumnName("SELFSERVICEACCESS");
            entity.Property(e => e.Selfservicelicense).HasColumnName("SELFSERVICELICENSE");
            entity.Property(e => e.SeqCountry).HasColumnName("SEQ_COUNTRY");
            entity.Property(e => e.SeqPriority).HasColumnName("SEQ_PRIORITY");
            entity.Property(e => e.SeqStaff).HasColumnName("SEQ_STAFF");
            entity.Property(e => e.SeqSurvey).HasColumnName("SEQ_SURVEY");
            entity.Property(e => e.Sid)
                .HasMaxLength(208)
                .HasColumnName("SID");
            entity.Property(e => e.Sspwd)
                .HasMaxLength(512)
                .HasColumnName("SSPWD");
            entity.Property(e => e.State)
                .HasMaxLength(25)
                .HasColumnName("STATE");
            entity.Property(e => e.SurveyCounter).HasColumnName("SURVEY_COUNTER");
            entity.Property(e => e.TimeZone)
                .HasMaxLength(10)
                .HasColumnName("TIME_ZONE");
            entity.Property(e => e.Usedept)
                .HasDefaultValue((short)0)
                .HasColumnName("USEDEPT:");
            entity.Property(e => e.Uselocation)
                .HasDefaultValue((short)0)
                .HasColumnName("USELOCATION:");
            entity.Property(e => e.Wiaenabled).HasColumnName("WIAENABLED");
            entity.Property(e => e.Winuserid)
                .HasMaxLength(85)
                .HasColumnName("WINUSERID");
            entity.Property(e => e.Zip)
                .HasMaxLength(10)
                .HasColumnName("ZIP");
        }
    }

}
