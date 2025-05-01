namespace Constellation.Application.Domains.SchoolContacts.Queries.GetContactSummary;

using Abstractions.Messaging;
using Core.Models.SchoolContacts.Identifiers;

public sealed record GetContactSummaryQuery(
    SchoolContactId ContactId)
    : IQuery<ContactSummaryResponse>;
