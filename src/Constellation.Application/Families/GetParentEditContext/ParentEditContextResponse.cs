namespace Constellation.Application.Families.GetParentEditContext;

using Constellation.Core.Models.Identifiers;

public sealed record ParentEditContextResponse(
    ParentId ParentId,
    FamilyId FamilyId,
    string Title,
    string FirstName,
    string LastName,
    string MobileNumber,
    string EmailAddress);