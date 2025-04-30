namespace Constellation.Application.StaffMembers.GetStaffMembersAsDictionary;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetStaffMembersAsDictionaryQuery()
    : IQuery<Dictionary<string, string>>;