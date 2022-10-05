using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations
{
    public class StudentReportConfiguration : IEntityTypeConfiguration<StudentReport>
    {
        public void Configure(EntityTypeBuilder<StudentReport> builder)
        {
            builder.HasKey(s => s.Id);

            builder.HasOne(s => s.Student).WithMany(s => s.Reports);
        }
    }
}