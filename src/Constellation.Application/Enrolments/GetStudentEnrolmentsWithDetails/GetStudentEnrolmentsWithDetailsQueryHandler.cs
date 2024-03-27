namespace Constellation.Application.Enrolments.GetStudentEnrolmentsWithDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Errors;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Shared;
using Core.Models.Enrolments.Repositories;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Subjects.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStudentEnrolmentsWithDetailsQueryHandler
    : IQueryHandler<GetStudentEnrolmentsWithDetailsQuery, List<StudentEnrolmentResponse>>
{
    private readonly IStaffRepository _staffRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ILogger _logger;

    public GetStudentEnrolmentsWithDetailsQueryHandler(
        IStaffRepository staffRepository,
        IEnrolmentRepository enrolmentRepository,
        IOfferingRepository offeringRepository,
        ICourseRepository courseRepository,
        Serilog.ILogger logger)
    {
        _staffRepository = staffRepository;
        _enrolmentRepository = enrolmentRepository;
        _offeringRepository = offeringRepository;
        _courseRepository = courseRepository;
        _logger = logger.ForContext<GetStudentEnrolmentsWithDetailsQuery>();
    }

    public async Task<Result<List<StudentEnrolmentResponse>>> Handle(GetStudentEnrolmentsWithDetailsQuery request, CancellationToken cancellationToken)
    {
        List<StudentEnrolmentResponse> returnData = new();

        var enrolments = await _enrolmentRepository.GetCurrentByStudentId(request.StudentId, cancellationToken);

        if (enrolments is null || !enrolments.Any())
        {
            _logger.Warning("No active enrolments found for student {id}", request.StudentId);

            return Result.Failure<List<StudentEnrolmentResponse>>(DomainErrors.Enrolments.Enrolment.NotFoundForStudent(request.StudentId));
        }

        foreach (var enrolment in enrolments)
        {
            var offering = await _offeringRepository.GetById(enrolment.OfferingId, cancellationToken);

            if (offering is null)
            {
                _logger.Warning("Could not find Offering with Id {id}", enrolment.OfferingId);

                continue;
            }

            var teachers = await _staffRepository.GetPrimaryTeachersForOffering(enrolment.OfferingId, cancellationToken);

            if (teachers is null || !teachers.Any())
            {
                _logger.Warning("Could not find teacher for offering {offering}", offering.Name);
            }

            var course = await _courseRepository.GetById(offering.CourseId, cancellationToken);

            if (course is null)
            {
                _logger.Warning("Could not find Course with Id {id}", offering.CourseId);

                continue;
            }

            returnData.Add(new(
                enrolment.OfferingId,
                offering.Name,
                course.Name,
                teachers?.Select(teacher => teacher.DisplayName).ToList(),
                false));
        }

        return returnData;
    }
}
