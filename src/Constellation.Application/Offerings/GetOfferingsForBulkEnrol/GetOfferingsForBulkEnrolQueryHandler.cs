namespace Constellation.Application.Offerings.GetOfferingsForBulkEnrol;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Shared;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetOfferingsForBulkEnrolQueryHandler
    : IQueryHandler<GetOfferingsForBulkEnrolQuery, List<BulkEnrolOfferingResponse>>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IFacultyRepository _facultyRepository;

    public GetOfferingsForBulkEnrolQueryHandler(
        IOfferingRepository offeringRepository,
        ICourseRepository courseRepository,
        IFacultyRepository facultyRepository)
    {
        _offeringRepository = offeringRepository;
        _courseRepository = courseRepository;
        _facultyRepository = facultyRepository;
    }

    public async Task<Result<List<BulkEnrolOfferingResponse>>> Handle(GetOfferingsForBulkEnrolQuery request, CancellationToken cancellationToken)
    {
        List<BulkEnrolOfferingResponse> response = new();

        List<Offering> offerings = await _offeringRepository.GetActiveByGrade(request.Grade, cancellationToken);

        foreach (Offering offering in offerings)
        {
            Course course = await _courseRepository.GetById(offering.CourseId, cancellationToken);

            if (course is null)
                continue;

            Faculty faculty = await _facultyRepository.GetById(course.FacultyId, cancellationToken);

            if (faculty is null)
                continue;

            response.Add(new(
                offering.Id,
                offering.Name,
                course.FacultyId,
                faculty.Name));
        }

        return response;
    }
}
