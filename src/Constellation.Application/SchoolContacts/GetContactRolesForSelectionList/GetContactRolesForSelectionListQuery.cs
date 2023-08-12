namespace Constellation.Application.SchoolContacts.GetContactRolesForSelectionList;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetContactRolesForSelectionListQuery()
    : IQuery<List<string>>;