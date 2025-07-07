namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffMembersAsDictionary;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using System.Collections.Generic;

public sealed record GetStaffMembersAsDictionaryQuery()
    : IQuery<Dictionary<StaffId, string>>;