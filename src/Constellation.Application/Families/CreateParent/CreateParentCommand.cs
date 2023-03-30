namespace Constellation.Application.Families.CreateParent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Families;
using Constellation.Core.Models.Identifiers;

public sealed record CreateParentCommand(
    FamilyId FamilyId,
    string Title,
    string FirstName,
    string LastName,
    string MobileNumber,
    string EmailAddress)
    : ICommand<Parent>;
