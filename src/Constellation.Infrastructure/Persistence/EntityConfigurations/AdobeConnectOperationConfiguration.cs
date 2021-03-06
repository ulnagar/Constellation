using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;

namespace Constellation.Infrastructure.Persistence.EntityConfigurations
{
    public class AdobeConnectOperationConfiguration : IEntityTypeConfiguration<AdobeConnectOperation>
    {
        public void Configure(EntityTypeBuilder<AdobeConnectOperation> builder)
        {
            builder.HasDiscriminator<string>("UserType")
            .HasValue<StudentAdobeConnectOperation>("Student")
            .HasValue<CasualAdobeConnectOperation>("Casual")
            .HasValue<TeacherAdobeConnectOperation>("Teacher")
            .HasValue<TeacherAdobeConnectGroupOperation>("TeacherGroup");

            builder.HasOne(o => o.Room).WithMany().HasForeignKey(o => o.ScoId);
        }
    }

    public class StudentAdobeConnectOperationConfiguration : IEntityTypeConfiguration<StudentAdobeConnectOperation>
    {
        public void Configure(EntityTypeBuilder<StudentAdobeConnectOperation> builder)
        {
            builder.HasOne(o => o.Student)
                .WithMany(s => s.AdobeConnectOperations)
                .HasForeignKey(o => o.StudentId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }

    public class CasualAdobeConnectOperationConfiguration : IEntityTypeConfiguration<CasualAdobeConnectOperation>
    {
        public void Configure(EntityTypeBuilder<CasualAdobeConnectOperation> builder)
        {
            builder.HasOne(o => o.Casual)
                .WithMany(c => c.AdobeConnectOperations)
                .HasForeignKey(o => o.CasualId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(o => o.Cover)
                .WithMany(c => c.AdobeConnectOperations as ICollection<CasualAdobeConnectOperation>)
                .HasForeignKey(o => o.CoverId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }

    public class TeacherAdobeConnectOperationConfiguration : IEntityTypeConfiguration<TeacherAdobeConnectOperation>
    {
        public void Configure(EntityTypeBuilder<TeacherAdobeConnectOperation> builder)
        {
            builder.HasOne(o => o.Teacher)
                .WithMany(s => s.AdobeConnectOperations)
                .HasForeignKey(o => o.StaffId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(o => o.Cover)
                .WithMany(s => s.AdobeConnectOperations as ICollection<TeacherAdobeConnectOperation>)
                .HasForeignKey(o => o.CoverId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }

    public class TeacherAdobeConnectGroupOperationConfiguration : IEntityTypeConfiguration<TeacherAdobeConnectGroupOperation>
    {
        public void Configure(EntityTypeBuilder<TeacherAdobeConnectGroupOperation> builder)
        {
            builder.HasOne(o => o.Teacher)
                .WithMany(s => s.AdobeConnectGroupOperations)
                .HasForeignKey(o => o.TeacherId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}