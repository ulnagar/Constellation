namespace Constellation.Application.Domains.Families.Commands.DeleteFamilyById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record DeleteFamilyByIdCommand(
    FamilyId FamilyId)
    : ICommand;
