namespace Constellation.Application.Domains.Families.Commands.DeleteParentById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record DeleteParentByIdCommand(
    FamilyId FamilyId,
    ParentId ParentId)
    : ICommand;
