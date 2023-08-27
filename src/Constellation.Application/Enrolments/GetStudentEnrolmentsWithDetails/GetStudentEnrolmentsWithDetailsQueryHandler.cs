namespace Constellation.Application.Enrolments.GetStudentEnrolmentsWithDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
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
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ILogger _logger;

    public GetStudentEnrolmentsWithDetailsQueryHandler(
        IStaffRepository staffRepository,
        IEnrolmentRepository enrolmentRepository,
        IOfferingRepository offeringRepository,
        Serilog.ILogger logger)
    {
        _staffRepository = staffRepository;
        _enrolmentRepository = enrolmentRepository;
        _offeringRepository = offeringRepository;
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

            returnData.Add(new(
                enrolment.OfferingId,
                offering.Name,
                offering.Course.Name,
                teachers?.Select(teacher => teacher.DisplayName).ToList(),
                false));
        }

        return returnData;
    }
}
