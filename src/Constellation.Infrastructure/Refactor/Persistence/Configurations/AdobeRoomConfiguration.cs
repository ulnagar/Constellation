namespace Constellation.Infrastructure.Refactor.Persistence.Configurations;

using Constellation.Core.Refactor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AdobeRoomConfiguration : IEntityTypeConfiguration<AdobeRoom>
{
    public void Configure(EntityTypeBuilder<AdobeRoom> builder)
    {
        
    }
}
