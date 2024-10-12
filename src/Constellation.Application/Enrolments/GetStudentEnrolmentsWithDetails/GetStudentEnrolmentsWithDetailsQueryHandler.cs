namespace Constellation.Application.Enrolments.GetStudentEnrolmentsWithDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Errors;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Shared;
using Core.Models;
using Core.Models.Enrolments;
using Core.Models.Enrolments.Repositories;
using Core.Models.Offerings;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Subjects;
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

        List<Enrolment> enrolments = await _enrolmentRepository.GetCurrentByStudentId(request.StudentId, cancellationToken);

        if (enrolments is null || !enrolments.Any())
        {
            _logger.Warning("No active enrolments found for student {id}", request.StudentId);

            return Result.Failure<List<StudentEnrolmentResponse>>(DomainErrors.Enrolments.Enrolment.NotFoundForStudent(request.StudentId));
        }

        foreach (Enrolment enrolment in enrolments)
        {
            Offering offering = await _offeringRepository.GetById(enrolment.OfferingId, cancellationToken);

            if (offering is null)
            {
                _logger.Warning("Could not find Offering with Id {id}", enrolment.OfferingId);

                continue;
            }

            List<Staff> teachers = await _staffRepository.GetPrimaryTeachersForOffering(enrolment.OfferingId, cancellationToken);

            if (teachers is null || !teachers.Any())
            {
                _logger.Warning("Could not find teacher for offering {offering}", offering.Name);
            }

            Course course = await _courseRepository.GetById(offering.CourseId, cancellationToken);

            if (course is null)
            {
                _logger.Warning("Could not find Course with Id {id}", offering.CourseId);

                continue;
            }

            List<StudentEnrolmentResponse.Resource> resources = offering.Resources
                .Select(entry =>
                    new StudentEnrolmentResponse.Resource(
                        entry.Type.Value, 
                        entry.Name, 
                        entry.Url))
                .ToList();

            returnData.Add(new(
                enrolment.OfferingId,
                offering.Name,
                course.Name,
                teachers?.Select(teacher => teacher.DisplayName).ToList(),
                resources,
                false));
        }

        return returnData;
    }
}
