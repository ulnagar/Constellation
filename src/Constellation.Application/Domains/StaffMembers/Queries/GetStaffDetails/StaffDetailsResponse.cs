﻿namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffDetails;

using Core.Models.Faculties.Identifiers;
using Core.Models.Offerings.Identifiers;
using Core.Models.SchoolContacts.Enums;
using Core.Models.SchoolContacts.Identifiers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.ValueObjects;
using Core.ValueObjects;
using System;
using System.Collections.Generic;

public sealed record StaffDetailsResponse(
    StaffId StaffId,
    EmployeeId EmployeeId,
    Name StaffName,
    EmailAddress EmailAddress,
    bool IsShared,
    bool IsDeleted,
    List<StaffDetailsResponse.SchoolAssignmentResponse> SchoolAssignments,
    List<StaffDetailsResponse.FacultyMembershipResponse> Faculties,
    List<StaffDetailsResponse.OfferingResponse> Offerings,
    List<StaffDetailsResponse.SessionResponse> Sessions,
    List<StaffDetailsResponse.SchoolContactResponse> Contacts)
{
    public sealed record SchoolAssignmentResponse(
        SchoolAssignmentId AssignmentId,
        string SchoolCode,
        string SchoolName,
        bool IsDeleted,
        DateOnly StartDate,
        DateOnly? EndDate);

    public sealed record FacultyMembershipResponse(
        FacultyMembershipId MembershipId,
        FacultyId FacultyId,
        string Name,
        string Role);

    public sealed record OfferingResponse(
        OfferingId OfferingId,
        string ClassName,
        string CourseName,
        string Role);

    public sealed record SessionResponse(
        SessionId SessionId,
        string PeriodName,
        string PeriodSortOrder,
        string ClassName,
        int Length);

    public sealed record SchoolContactResponse(
        SchoolContactId ContactId,
        string Name,
        string EmailAddress,
        string PhoneNumber,
        Position Role,
        string School);
}