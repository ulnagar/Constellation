using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.EntityConfigurations
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.HasKey(s => s.StudentId);

            builder.Property(s => s.Gender)
                .HasMaxLength(1);

            builder.HasOne(s => s.School)
                .WithMany(s => s.Students);

            builder.HasMany(s => s.AdobeConnectOperations);

            builder.HasOne(s => s.Family).WithMany(s => s.Students);
        }
    }
}