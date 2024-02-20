namespace Constellation.Application.SchoolContacts.GetContactSummary;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.SchoolContacts.Identifiers;

public sealed record GetContactSummaryQuery(
    SchoolContactId ContactId)
    : IQuery<ContactSummaryResponse>;
