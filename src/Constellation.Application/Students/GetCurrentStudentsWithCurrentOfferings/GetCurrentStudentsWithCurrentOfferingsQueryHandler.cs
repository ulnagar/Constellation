namespace Constellation.Application.Students.GetCurrentStudentsWithCurrentOfferings;

using Abstractions.Messaging;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Enrolments;
using Core.Models.Enrolments.Repositories;
using Core.Models.Offerings;
using Core.Models.Offerings.Repositories;
using Core.Models.Students;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCurrentStudentsWithCurrentOfferingsQueryHandler
    : IQueryHandler<GetCurrentStudentsWithCurrentOfferingsQuery, List<StudentWithOfferingsResponse>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ILogger _logger;

    public GetCurrentStudentsWithCurrentOfferingsQueryHandler(
        IStudentRepository studentRepository,
        IEnrolmentRepository enrolmentRepository,
        IOfferingRepository offeringRepository,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _enrolmentRepository = enrolmentRepository;
        _offeringRepository = offeringRepository;
        _logger = logger;
    }

    public async Task<Result<List<StudentWithOfferingsResponse>>> Handle(GetCurrentStudentsWithCurrentOfferingsQuery request, CancellationToken cancellationToken)
    {
        List<StudentWithOfferingsResponse> response = new();

        List<Student> students = await _studentRepository.GetCurrentStudentsWithSchool(cancellationToken);

        List<Offering> offerings = await _offeringRepository.GetAll(cancellationToken);

        foreach (Student student in students)
        {
            List<StudentWithOfferingsResponse.OfferingResponse> studentOfferings = new();

            List<Enrolment> enrolments = await _enrolmentRepository.GetCurrentByStudentId(student.StudentId, cancellationToken);

            foreach (Enrolment enrolment in enrolments)
            {
                Offering offering = offerings.FirstOrDefault(offering => offering.Id == enrolment.OfferingId);

                if (offering is null)
                    continue;

                studentOfferings.Add(new(
                    offering.Id,
                    offering.Name,
                    offering.IsCurrent));
            }

            response.Add(new(
                student.StudentId,
                student.GetName(),
                student.School.Name,
                student.CurrentGrade,
                studentOfferings));
        }

        return response;
    }
}
