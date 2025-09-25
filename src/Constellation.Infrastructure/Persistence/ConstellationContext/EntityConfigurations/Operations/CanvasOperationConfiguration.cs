namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Operations;

using Constellation.Core.Models.Operations;
using Core.Models.Canvas.Models;
using Core.Models.Operations.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class CanvasOperationConfiguration : IEntityTypeConfiguration<CanvasOperation>
{
    public void Configure(EntityTypeBuilder<CanvasOperation> builder)
    {
        builder.ToTable("CanvasOperations");

        builder.HasDiscriminator<string>("OperationType")
            .HasValue<CreateUserCanvasOperation>("CreateUser")
            .HasValue<ModifyEnrolmentCanvasOperation>("ModifyEnrolment")
            .HasValue<DeleteUserCanvasOperation>("DeleteUser")
            .HasValue<UpdateUserEmailCanvasOperation>("UpdateEmail");
    }
}

internal sealed class CreateUserCanvasOperationConfiguration : IEntityTypeConfiguration<CreateUserCanvasOperation>
{
    public void Configure(EntityTypeBuilder<CreateUserCanvasOperation> builder)
    {
        builder
            .Property(operation => operation.PortalUsername)
            .HasColumnName(nameof(CreateUserCanvasOperation.PortalUsername));
    }
}

internal sealed class ModifyEnrolmentCanvasOperationConfiguration : IEntityTypeConfiguration<ModifyEnrolmentCanvasOperation>
{
    public void Configure(EntityTypeBuilder<ModifyEnrolmentCanvasOperation> builder)
    {
        builder
            .Property(operation => operation.Action)
            .HasConversion(
                action => action.Value,
                value => CanvasAction.FromValue(value));

        builder
            .Property(operation => operation.UserType)
            .HasConversion(
                user => user.Value,
                value => CanvasUserType.FromValue(value));

        builder
            .Property(operation => operation.CourseId)
            .HasConversion(
                courseId => courseId.ToString(),
                value => CanvasCourseCode.FromValue(value));

        builder
            .Property(operation => operation.SectionId)
            .HasConversion(
                sectionId => sectionId.ToString(),
                value => CanvasSectionCode.FromValue(value));
    }
}

internal sealed class UpdateUserEmailCanvasOperationConfiguration : IEntityTypeConfiguration<UpdateUserEmailCanvasOperation>
{
    public void Configure(EntityTypeBuilder<UpdateUserEmailCanvasOperation> builder)
    {
        builder
            .Property(operation => operation.PortalUsername)
            .HasColumnName(nameof(UpdateUserEmailCanvasOperation.PortalUsername));
    }
}