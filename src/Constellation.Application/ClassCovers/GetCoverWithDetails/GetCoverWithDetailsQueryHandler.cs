namespace Constellation.Application.ClassCovers.GetCoverWithDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCoverWithDetailsQueryHandler
    : IQueryHandler<GetCoverWithDetailsQuery, CoverWithDetailsResponse>
{
    private readonly IClassCoverRepository _classCoverRepository;
    private readonly ICourseOfferingRepository _offeringRepository;
    private readonly ICasualRepository _casualRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolRepository _schoolRepository;

    public GetCoverWithDetailsQueryHandler(
        IClassCoverRepository classCoverRepository,
        ICourseOfferingRepository offeringRepository,
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

    public async Task<Result<CoverWithDetailsResponse>> Handle(GetCoverWithDetailsQuery request, CancellationToken cancellationToken)
    {
        var cover = await _classCoverRepository.GetById(request.Id, cancellationToken);

        if (cover is null)
        {
            return Result.Failure<CoverWithDetailsResponse>(DomainErrors.ClassCovers.Cover.NotFound(request.Id.Value));
        }

        var offering = await _offeringRepository
                .GetById(cover.OfferingId, cancellationToken);

        var offeringName = offering is null ? "" : offering.Name;

        string teacherName = "";
        string teacherSchool = "";

        if (cover.TeacherType == CoverTeacherType.Casual)
        {
            var teacher = await _casualRepository.GetById(Guid.Parse(cover.TeacherId), cancellationToken);

            if (teacher is not null)
            {
                teacherName = teacher.DisplayName;

                var school = await _schoolRepository.GetById(teacher.SchoolCode, cancellationToken);

                teacherSchool = school?.Name;
            }
        }
        else
        {
            var teacher = await _staffRepository.GetById(cover.TeacherId, cancellationToken);

            if (teacher is not null)
            {
                teacherName = teacher.DisplayName;

                var school = await _schoolRepository.GetById(teacher.SchoolCode, cancellationToken);

                teacherSchool = school?.Name;
            }
        }

        return new CoverWithDetailsResponse(
            cover.Id,
            teacherName,
            teacherSchool,
            offeringName,
            cover.StartDate,
            cover.EndDate);
    }
}
