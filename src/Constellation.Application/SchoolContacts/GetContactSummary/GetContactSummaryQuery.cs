namespace Constellation.Application.SchoolContacts.GetContactSummary;

using Constellation.Application.Abstractions.Messaging;

public sealed record GetContactSummaryQuery(
    int ContactId)
    : IQuery<ContactSummaryResponse>;
