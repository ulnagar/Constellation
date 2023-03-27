namespace Constellation.Application.Families.UpdateFamily;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record UpdateFamilyCommand(
    FamilyId FamilyId,
    string FamilyTitle,
    string AddressLine1,
    string AddressLine2,
    string AddressTown,
    string AddressPostCode,
    string FamilyEmail)
    : ICommand;
