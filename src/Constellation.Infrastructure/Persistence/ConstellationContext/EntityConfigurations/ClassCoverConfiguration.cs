using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations
{
    public class ClassCoverConfiguration : IEntityTypeConfiguration<ClassCover>
    {
        public void Configure(EntityTypeBuilder<ClassCover> builder)
        {
            builder.HasDiscriminator<string>("UserType")
                .HasValue<CasualClassCover>("Casual")
                .HasValue<TeacherClassCover>("Teacher");

            builder.HasMany(cover => cover.AdobeConnectOperations)
                .WithOne(operation => operation.Cover)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }

    public class CasualClassCoverConfiguration : IEntityTypeConfiguration<CasualClassCover>
    {

        public void Configure(EntityTypeBuilder<CasualClassCover> builder)
        {
            builder.HasOne(v => v.Casual)
                .WithMany(c => c.ClassCovers)
                .HasForeignKey(v => v.CasualId);

            builder.HasOne(v => v.Offering)
                .WithMany(o => (ICollection<CasualClassCover>)o.ClassCovers)
                .HasForeignKey(v => v.OfferingId);
        }
    }

    public class TeacherClassCoverConfiguration : IEntityTypeConfiguration<TeacherClassCover>
    {
        public void Configure(EntityTypeBuilder<TeacherClassCover> builder)
        {
            builder.HasOne(v => v.Staff)
                .WithMany(c => c.ClassCovers)
                .HasForeignKey(v => v.StaffId);

            builder.HasOne(v => v.Offering)
                .WithMany(o => (ICollection<TeacherClassCover>)o.ClassCovers)
                .HasForeignKey(v => v.OfferingId);
        }
    }
}