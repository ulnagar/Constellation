namespace Constellation.Infrastructure.Refactor.Persistence.Configurations;

using Constellation.Core.Refactor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CanvasCourseConfiguration : IEntityTypeConfiguration<CanvasCourse>
{
    public void Configure(EntityTypeBuilder<CanvasCourse> builder)
    {
        
    }
}
