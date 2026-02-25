namespace Constellation.Application.Domains.AppSettings.Queries.BuildStaffDictionary;

using Abstractions.Messaging;
using Constellation.Core.Enums;
using Constellation.Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers;
using System.Collections.Generic;

public sealed record BuildStaffDictionaryQuery(
    Dictionary<StaffId, List<Grade>> StaffIdList)
    : IQuery<Dictionary<StaffMember, List<Grade>>>;