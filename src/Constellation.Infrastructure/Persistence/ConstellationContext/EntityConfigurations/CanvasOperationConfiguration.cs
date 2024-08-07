﻿using Constellation.Core.Models.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations
{
    using Core.Models.Operations.Enums;

    public class CanvasOperationConfiguration : IEntityTypeConfiguration<CanvasOperation>
    {
        public void Configure(EntityTypeBuilder<CanvasOperation> builder)
        {
            builder.ToTable("CanvasOperations");

            builder.HasDiscriminator<string>("OperationType")
                .HasValue<CreateUserCanvasOperation>("CreateUser")
                .HasValue<ModifyEnrolmentCanvasOperation>("ModifyEnrolment")
                .HasValue<DeleteUserCanvasOperation>("DeleteUser");
        }
    }

    public class ModifyEnrolmentCanvasOperationConfiguration : IEntityTypeConfiguration<ModifyEnrolmentCanvasOperation>
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
        }
    }
}
