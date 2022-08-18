using Constellation.Infrastructure.Persistence.TrackItContext.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.TrackItContext.EntityConfigurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.HasKey(e => e.Sequence).HasName("PK_CUSTOMER_");
            builder.ToTable("_CUSTOMER_", "_SMDBA_");
            builder.HasIndex(e => e.Winuserid, "CUSTOMER_WINUSERID");
            builder.HasIndex(e => new { e.Fname, e.Sequence }, "Client First Name");
            builder.HasIndex(e => new { e.Name, e.Sequence }, "Client Last Name");
            builder.HasIndex(e => new { e.BillTo, e.Sequence }, "FK_CUSTOMER_BILL TO");
            builder.HasIndex(e => new { e.Dept, e.Sequence }, "FK_CUSTOMER_DEPT");
            builder.HasIndex(e => new { e.Location, e.Sequence }, "FK_CUSTOMER_LOCATION");
            builder.HasIndex(e => new { e.SeqCountry, e.Sequence }, "FK_CUSTOMER_SEQ_COUNTRY");
            builder.HasIndex(e => e.SeqPriority, "FK_CUSTOMER_SEQ_PRIORITY");
            builder.HasIndex(e => new { e.SeqStaff, e.Sequence }, "FK_CUSTOMER_SEQ_STAFF");
            builder.HasIndex(e => new { e.Group, e.Sequence }, "FK_CUSTOMER__GROUP_");
            builder.HasIndex(e => new { e.Owner, e.Sequence }, "FK_CUSTOMER__OWNER_");
            builder.HasIndex(e => e.Client, "UQ_CUSTOMER_CLIENT").IsUnique();
            builder.Property(e => e.Sequence).ValueGeneratedNever().HasColumnName("SEQUENCE");
            builder.Property(e => e.Address).HasMaxLength(128).HasColumnName("ADDRESS");
            builder.Property(e => e.BillTo).HasColumnName("BILL TO");
            builder.Property(e => e.Bldng).HasMaxLength(10).HasColumnName("BLDNG");
            builder.Property(e => e.City).HasMaxLength(20).HasColumnName("CITY");
            builder.Property(e => e.CliCcdt01).HasColumnType("datetime").HasColumnName("CLI_CCDT01");
            builder.Property(e => e.CliCcdt02).HasColumnType("datetime").HasColumnName("CLI_CCDT02");
            builder.Property(e => e.CliCcint01).HasColumnName("CLI_CCINT01");
            builder.Property(e => e.CliCcint02).HasColumnName("CLI_CCINT02");
            builder.Property(e => e.CliCctxt01).HasMaxLength(80).HasColumnName("CLI_CCTXT01");
            builder.Property(e => e.CliCctxt02).HasMaxLength(80).HasColumnName("CLI_CCTXT02");
            builder.Property(e => e.CliCctxt03).HasMaxLength(80).HasColumnName("CLI_CCTXT03");
            builder.Property(e => e.CliCctxt04).HasMaxLength(80).HasColumnName("CLI_CCTXT04");
            builder.Property(e => e.CliCctxt05).HasMaxLength(80).HasColumnName("CLI_CCTXT05");
            builder.Property(e => e.CliCctxt06).HasMaxLength(80).HasColumnName("CLI_CCTXT06");
            builder.Property(e => e.Client).IsRequired().HasMaxLength(255).HasColumnName("CLIENT");
            builder.Property(e => e.Country).HasMaxLength(25).HasColumnName("COUNTRY");
            builder.Property(e => e.Createdfromssd).HasColumnName("CREATEDFROMSSD:").HasDefaultValueSql("((0))");
            builder.Property(e => e.Dept).HasColumnName("DEPT");
            builder.Property(e => e.Displayclientcomments).HasColumnName("DISPLAYCLIENTCOMMENTS:").HasDefaultValueSql("((0))");
            builder.Property(e => e.DoNotSurvey).HasColumnName("DO_NOT_SURVEY");
            builder.Property(e => e.Emailid).HasMaxLength(1024).HasColumnName("_EMAILID_");
            builder.Property(e => e.Ext).HasMaxLength(5).HasColumnName("EXT");
            builder.Property(e => e.Fax).HasMaxLength(30).HasColumnName("FAX");
            builder.Property(e => e.Fname).HasMaxLength(50).HasColumnName("FNAME");
            builder.Property(e => e.Group).HasColumnName("_GROUP_");
            builder.Property(e => e.Inactive).HasColumnName("_INACTIVE_:");
            builder.Property(e => e.LastSurveyed).HasColumnType("datetime").HasColumnName("LAST_SURVEYED");
            builder.Property(e => e.Lastmodified).HasColumnType("datetime").HasColumnName("LASTMODIFIED").HasDefaultValueSql("(getdate())");
            builder.Property(e => e.Lastuser).HasMaxLength(255).HasColumnName("LASTUSER").HasDefaultValueSql("(user_name())");
            builder.Property(e => e.Location).HasColumnName("LOCATION");
            builder.Property(e => e.Logininfo).HasMaxLength(50).HasColumnName("LOGININFO");
            builder.Property(e => e.Mail).HasMaxLength(10).HasColumnName("MAIL");
            builder.Property(e => e.Mname).HasMaxLength(1).HasColumnName("MNAME").IsFixedLength(true);
            builder.Property(e => e.Name).HasMaxLength(50).HasColumnName("NAME");
            builder.Property(e => e.Note).HasColumnName("NOTE");
            builder.Property(e => e.NumSurveys).HasColumnName("NUM_SURVEYS");
            builder.Property(e => e.Owner).HasColumnName("_OWNER_");
            builder.Property(e => e.Ownerperms).HasColumnName("_OWNERPERMS_");
            builder.Property(e => e.Phone).HasMaxLength(30).HasColumnName("PHONE");
            builder.Property(e => e.Picture).HasMaxLength(13).HasColumnName("PICTURE");
            builder.Property(e => e.Position).HasMaxLength(100).HasColumnName("POSITION");
            builder.Property(e => e.Salt).HasMaxLength(512).HasColumnName("SALT");
            builder.Property(e => e.Selfserviceaccess).HasMaxLength(80).HasColumnName("SELFSERVICEACCESS");
            builder.Property(e => e.Selfservicelicense).HasColumnName("SELFSERVICELICENSE");
            builder.Property(e => e.SeqCountry).HasColumnName("SEQ_COUNTRY");
            builder.Property(e => e.SeqPriority).HasColumnName("SEQ_PRIORITY");
            builder.Property(e => e.SeqStaff).HasColumnName("SEQ_STAFF");
            builder.Property(e => e.SeqSurvey).HasColumnName("SEQ_SURVEY");
            builder.Property(e => e.Sid).HasMaxLength(208).HasColumnName("SID");
            builder.Property(e => e.Sspwd).HasMaxLength(512).HasColumnName("SSPWD");
            builder.Property(e => e.State).HasMaxLength(25).HasColumnName("STATE");
            builder.Property(e => e.SurveyCounter).HasColumnName("SURVEY_COUNTER");
            builder.Property(e => e.TimeZone).HasMaxLength(10).HasColumnName("TIME_ZONE");
            builder.Property(e => e.Usedept).HasColumnName("USEDEPT:").HasDefaultValueSql("((0))");
            builder.Property(e => e.Uselocation).HasColumnName("USELOCATION:").HasDefaultValueSql("((0))");
            builder.Property(e => e.Wiaenabled).HasColumnName("WIAENABLED");
            builder.Property(e => e.Winuserid).HasMaxLength(85).HasColumnName("WINUSERID");
            builder.Property(e => e.Zip).HasMaxLength(10).HasColumnName("ZIP");
        }
    }

}
