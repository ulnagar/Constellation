namespace Constellation.Application.Domains.Covers.Queries.GetAllCurrentAndFutureCovers;

using Abstractions.Messaging;
using Constellation.Core.Models.Covers;
using Constellation.Core.Models.Covers.Enums;
using Constellation.Core.Models.Covers.Repositories;
using Core.Abstractions.Repositories;
using Core.Models;
using Core.Models.Identifiers;
using Core.Models.Offerings.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAllCurrentAndFutureCoversQueryHandler
    : IQueryHandler<GetAllCurrentAndFutureCoversQuery, List<CoversListResponse>>
{
    private readonly ICoverRepository _coverRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICasualRepository _casualRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolRepository _schoolRepository;

    public GetAllCurrentAndFutureCoversQueryHandler(
        ICoverRepository coverRepository,
        IOfferingRepository offeringRepository,
        ICasualRepository casualRepository,
        IStaffRepository staffRepository,
        ISchoolRepository schoolRepository
        )
    {
        _coverRepository = coverRepository;
        _offeringRepository = offeringRepository;
        _casualRepository = casualRepository;
        _staffRepository = staffRepository;
        _schoolRepository = schoolRepository;
    }

    public async Task<Result<List<CoversListResponse>>> Handle(GetAllCurrentAndFutureCoversQuery request, CancellationToken cancellationToken)
    {
        List<CoversListResponse> returnData = new();

        var covers = await _coverRepository
            .GetAllCurrentAndUpcoming(cancellationToken);

        if (covers.Count == 0)
        {
            return returnData;
        }

        foreach (var cover in covers)
        {
            var offering = await _offeringRepository
                .GetById(cover.OfferingId, cancellationToken);

            var offeringName = offering is null ? "" : offering.Name;

            string teacherName = "";
            string teacherSchool = "";

            if (cover.TeacherType == CoverTeacherType.Casual)
            {
                var teacher = await _casualRepository.GetById(CasualId.FromValue(Guid.Parse(cover.TeacherId)), cancellationToken);

                if (teacher is null)
                {
                    continue;
                }

                teacherName = teacher.Name.DisplayName;

                var school = await _schoolRepository.GetById(teacher.SchoolCode, cancellationToken);

                if (school is null)
                {
                    continue;
                }

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

            CoverType coverType = cover switch
            {
                ClassCover => CoverType.ClassCover,
                AccessCover => CoverType.AccessCover,
                _ => null
            };


            var entry = new CoversListResponse(
                cover.Id,
                offeringName,
                cover.TeacherId,
                teacherName,
                teacherSchool,
                cover.TeacherType,
                cover.StartDate,
                cover.EndDate,
                coverType,
                (cover.StartDate <= DateOnly.FromDateTime(DateTime.Today) && cover.EndDate >= DateOnly.FromDateTime(DateTime.Today)),
                (cover.StartDate > DateOnly.FromDateTime(DateTime.Today)));

            returnData.Add(entry);
        }

        return returnData;
    }
}
