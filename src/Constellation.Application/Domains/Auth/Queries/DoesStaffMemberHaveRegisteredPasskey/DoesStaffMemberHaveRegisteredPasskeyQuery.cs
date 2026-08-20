namespace Constellation.Application.Domains.Auth.Queries.DoesStaffMemberHaveRegisteredPasskey;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;

public sealed record DoesStaffMemberHaveRegisteredPasskeyQuery(
    string EmailAddress)
    : IQuery<bool>;