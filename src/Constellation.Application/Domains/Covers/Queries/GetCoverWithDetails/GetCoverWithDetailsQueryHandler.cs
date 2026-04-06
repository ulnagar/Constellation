namespace Constellation.Application.Domains.Covers.Queries.GetCoverWithDetails;

using Abstractions.Messaging;
using Constellation.Core.Models.Covers;
using Constellation.Core.Models.Covers.Enums;
using Constellation.Core.Models.Covers.Repositories;
using Core.Abstractions.Repositories;
using Core.Models;
using Core.Models.Casuals;
using Core.Models.Covers.Errors;
using Core.Models.Identifiers;
using Core.Models.Offerings;
using Core.Models.Offerings.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCoverWithDetailsQueryHandler
    : IQueryHandler<GetCoverWithDetailsQuery, CoverWithDetailsResponse>
{
    private readonly ICoverRepository _coverRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICasualRepository _casualRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolRepository _schoolRepository;

    public GetCoverWithDetailsQueryHandler(
        ICoverRepository coverRepository,
        IOfferingRepository offeringRepository,
        ICasualRepository casualRepository,
        IStaffRepository staffRepository,
        ISchoolRepository schoolRepository)
    {
        _coverRepository = coverRepository;
        _offeringRepository = offeringRepository;
        _casualRepository = casualRepository;
        _staffRepository = staffRepository;
        _schoolRepository = schoolRepository;
    }

    public async Task<Result<CoverWithDetailsResponse>> Handle(GetCoverWithDetailsQuery request, CancellationToken cancellationToken)
    {
        Cover? cover = await _coverRepository.GetById(request.Id, cancellationToken);

        if (cover is null)
        {
            return Result.Failure<CoverWithDetailsResponse>(CoverErrors.NotFound(request.Id));
        }

        Offering? offering = await _offeringRepository
                .GetById(cover.OfferingId, cancellationToken);

        string offeringName = offering?.Name ?? "";

        string teacherName = "";
        string teacherSchool = "";

        if (cover.TeacherType == CoverTeacherType.Casual)
        {
            Casual? teacher = await _casualRepository.GetById(CasualId.FromValue(Guid.Parse(cover.TeacherId)), cancellationToken);

            if (teacher is not null)
            {
                teacherName = teacher.Name.DisplayName;

                School? school = await _schoolRepository.GetById(teacher.SchoolCode, cancellationToken);

                teacherSchool = school?.Name ?? string.Empty;
            }
        }
        else
        {
            StaffId staffId = StaffId.FromValue(Guid.Parse(cover.TeacherId));

            StaffMember? teacher = staffId == StaffId.Empty
                ? null
                : await _staffRepository.GetById(staffId, cancellationToken);

            if (teacher is not null)
            {
                teacherName = teacher.Name.DisplayName;

                teacherSchool = (teacher.CurrentAssignment is null)
                    ? string.Empty
                    : (await _schoolRepository.GetById(teacher.CurrentAssignment.SchoolCode, cancellationToken))?.Name ?? string.Empty;
            }
        }

        CoverType? coverType = cover switch
        {
            ClassCover => CoverType.ClassCover,
            AccessCover => CoverType.AccessCover,
            _ => null
        };

        return new CoverWithDetailsResponse(
            cover.Id,
            teacherName,
            teacherSchool,
            offeringName,
            cover.StartDate,
            cover.EndDate,
            coverType);
    }
}
