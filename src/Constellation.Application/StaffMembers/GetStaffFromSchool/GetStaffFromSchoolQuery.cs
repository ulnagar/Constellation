namespace Constellation.Application.StaffMembers.GetStaffFromSchool;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetStaffFromSchoolQuery(
    string SchoolCode) 
    : IQuery<List<StaffSelectionListResponse>>;