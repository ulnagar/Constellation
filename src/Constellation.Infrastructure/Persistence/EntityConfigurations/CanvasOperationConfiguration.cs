using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.EntityConfigurations
{
    public class CanvasOperationConfiguration : IEntityTypeConfiguration<CanvasOperation>
    {
        public void Configure(EntityTypeBuilder<CanvasOperation> builder)
        {
            builder.HasDiscriminator<string>("OperationType")
                .HasValue<CreateUserCanvasOperation>("CreateUser")
                .HasValue<ModifyEnrolmentCanvasOperation>("ModifyEnrolment")
                .HasValue<DeleteUserCanvasOperation>("DeleteUser");
        }
    }
}
