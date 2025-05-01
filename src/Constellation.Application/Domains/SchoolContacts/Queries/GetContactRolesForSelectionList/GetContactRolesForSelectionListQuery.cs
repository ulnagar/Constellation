namespace Constellation.Application.Domains.SchoolContacts.Queries.GetContactRolesForSelectionList;

using Abstractions.Messaging;
using Core.Models.SchoolContacts.Enums;
using System.Collections.Generic;

public sealed record GetContactRolesForSelectionListQuery(
    bool IncludeRestrictedContacts = false)
    : IQuery<List<Position>>;