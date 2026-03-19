namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffFromSchool;

using Abstractions.Messaging;
using Core.Models.Identifiers;
using Models;
using System.Collections.Generic;

public sealed record GetStaffFromSchoolQuery(
    SchoolCode SchoolCode) 
    : IQuery<List<StaffSelectionListResponse>>;