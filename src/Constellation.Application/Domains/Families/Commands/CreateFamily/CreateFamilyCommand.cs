namespace Constellation.Application.Domains.Families.Commands.CreateFamily;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Families;
using Constellation.Core.ValueObjects;

public sealed record CreateFamilyCommand(
    string FamilyTitle,
    string AddressLine1,
    string AddressLine2,
    string AddressTown,
    string AddressPostCode,
    EmailAddress FamilyEmail)
    : ICommand<Family>;
