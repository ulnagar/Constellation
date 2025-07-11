﻿namespace Constellation.Application.Domains.ClassCovers.Queries.GetAllCoversForCalendarYear;

using Abstractions.Messaging;
using Constellation.Core.Models.StaffMembers;
using Constellation.Core.Models.StaffMembers.Identifiers;
using Core.Abstractions.Repositories;
using Core.Models;
using Core.Models.Casuals;
using Core.Models.Covers;
using Core.Models.Identifiers;
using Core.Models.Offerings;
using Core.Models.Offerings.Repositories;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Repositories;
using Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAllCoversForCalendarYearQueryHandler
    : IQueryHandler<GetAllCoversForCalendarYearQuery, List<CoversListResponse>>
{
    private readonly IClassCoverRepository _classCoverRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICasualRepository _casualRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolRepository _schoolRepository;

    public GetAllCoversForCalendarYearQueryHandler(
        IClassCoverRepository classCoverRepository,
        IOfferingRepository offeringRepository,
        ICasualRepository casualRepository,
        IStaffRepository staffRepository,
        ISchoolRepository schoolRepository)
	{
        _classCoverRepository = classCoverRepository;
        _offeringRepository = offeringRepository;
        _casualRepository = casualRepository;
        _staffRepository = staffRepository;
        _schoolRepository = schoolRepository;
    }

    public async Task<Result<List<CoversListResponse>>> Handle(GetAllCoversForCalendarYearQuery request, CancellationToken cancellationToken)
    {
        List<CoversListResponse> returnData = new();

        List<ClassCover> covers = await _classCoverRepository.GetAllForCurrentCalendarYear(cancellationToken);

        if (covers.Count == 0)
            return returnData;

        foreach (ClassCover cover in covers)
        {
            Offering offering = await _offeringRepository.GetById(cover.OfferingId, cancellationToken);

            string offeringName = offering?.Name ?? "";
            string teacherName = "";
            string teacherSchool = "";

            if (cover.TeacherType == CoverTeacherType.Casual)
            {
                Casual teacher = await _casualRepository.GetById(CasualId.FromValue(Guid.Parse(cover.TeacherId)), cancellationToken);

                if (teacher is null)
                    continue;

                teacherName = teacher.Name.DisplayName;

                School school = await _schoolRepository.GetById(teacher.SchoolCode, cancellationToken);

                if (school is null)
                    continue;

                teacherSchool = school.Name;
            }
            else
            {
                StaffId staffId = StaffId.FromValue(Guid.Parse(cover.TeacherId));

                StaffMember teacher = staffId == StaffId.Empty
                    ? null
                    : await _staffRepository.GetById(staffId, cancellationToken);

                if (teacher is null)
                    continue;

                teacherName = teacher.Name.DisplayName;

                if (teacher.CurrentAssignment is null)
                    continue;

                School school = await _schoolRepository.GetById(teacher.CurrentAssignment.SchoolCode, cancellationToken);

                if (school is null)
                    continue;

                teacherSchool = school.Name;
            }

            CoversListResponse entry = new (
                cover.Id,
                offeringName,
                cover.TeacherId,
                teacherName,
                teacherSchool,
                cover.TeacherType,
                cover.StartDate,
                cover.EndDate,
                !cover.IsDeleted && (cover.StartDate <= DateOnly.FromDateTime(DateTime.Today) && cover.EndDate >= DateOnly.FromDateTime(DateTime.Today)),
                !cover.IsDeleted && (cover.StartDate > DateOnly.FromDateTime(DateTime.Today)));

            returnData.Add(entry);
        }

        return returnData;
    }
}
