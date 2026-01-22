namespace Constellation.Application.Domains.EmergencyConsole.Queries.GetContactDetails;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetContactDetailsQuery()
    : IQuery<List<ContactDetail>>;