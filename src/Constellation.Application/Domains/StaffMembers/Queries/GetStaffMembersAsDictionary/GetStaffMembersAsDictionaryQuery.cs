namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffMembersAsDictionary;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetStaffMembersAsDictionaryQuery()
    : IQuery<Dictionary<string, string>>;