namespace Constellation.Application.OfferingEnrolments.GetStudentEnrolmentsWithDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.OfferingEnrolments;
using Constellation.Core.Models.OfferingEnrolments.Repositories;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Repositories;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStudentEnrolmentsWithDetailsQueryHandler
    : IQueryHandler<GetStudentEnrolmentsWithDetailsQuery, List<StudentEnrolmentResponse>>
{
    private readonly IStaffRepository _staffRepository;
    private readonly IOfferingEnrolmentRepository _offeringEnrolmentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ILogger _logger;

    public GetStudentEnrolmentsWithDetailsQueryHandler(
        IStaffRepository staffRepository,
        IOfferingEnrolmentRepository offeringEnrolmentRepository,
        IOfferingRepository offeringRepository,
        ICourseRepository courseRepository,
        Serilog.ILogger logger)
    {
        _staffRepository = staffRepository;
        _offeringEnrolmentRepository = offeringEnrolmentRepository;
        _offeringRepository = offeringRepository;
        _courseRepository = courseRepository;
        _logger = logger.ForContext<GetStudentEnrolmentsWithDetailsQuery>();
    }

    public async Task<Result<List<StudentEnrolmentResponse>>> Handle(GetStudentEnrolmentsWithDetailsQuery request, CancellationToken cancellationToken)
    {
        List<StudentEnrolmentResponse> returnData = new();

        List<OfferingEnrolment> enrolments = await _offeringEnrolmentRepository.GetCurrentByStudentId(request.StudentId, cancellationToken);

        if (enrolments is null || !enrolments.Any())
        {
            _logger.Warning("No active enrolments found for student {id}", request.StudentId);

            return Result.Failure<List<StudentEnrolmentResponse>>(DomainErrors.Enrolments.Enrolment.NotFoundForStudent(request.StudentId));
        }

        foreach (OfferingEnrolment enrolment in enrolments)
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
