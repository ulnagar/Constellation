namespace Constellation.Application.Domains.Edval.Events.EdvalClassMembershipsUpdated;

using Application.Abstractions.Messaging;
using Constellation.Core.Models.Edval;
using Constellation.Core.Models.Edval.Enums;
using Constellation.Core.Models.Edval.Events;
using Constellation.Core.Models.Offerings.Repositories;
using Core.Models.Enrolments;
using Core.Models.Enrolments.Repositories;
using Core.Models.Offerings;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Models.Students.ValueObjects;
using Core.Shared;
using Interfaces.Repositories;
using Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CalculateDifferences : IIntegrationEventHandler<EdvalClassMembershipsUpdatedIntegrationEvent>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IEdvalRepository _edvalRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CalculateDifferences(
        IEdvalRepository edvalRepository,
        IOfferingRepository offeringRepository,
        IStudentRepository studentRepository,
        IEnrolmentRepository enrolmentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _edvalRepository = edvalRepository;
        _offeringRepository = offeringRepository;
        _enrolmentRepository = enrolmentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<EdvalClassMembershipsUpdatedIntegrationEvent>();
    }
    public async Task Handle(EdvalClassMembershipsUpdatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        List<Student> existingStudents = await _studentRepository.GetCurrentStudents(cancellationToken);
        List<Offering> existingOfferings = await _offeringRepository.GetAllActive(cancellationToken);
        List<Enrolment> existingEnrolments = await _enrolmentRepository.GetCurrent(cancellationToken);

        List<EdvalClassMembership> edvalClassMemberships = await _edvalRepository.GetClassMemberships(cancellationToken);

        foreach (EdvalClassMembership membership in edvalClassMemberships)
        {
            Result<StudentReferenceNumber> srn = StudentReferenceNumber.Create(membership.StudentId);

            if (srn.IsFailure)
            {
                _logger
                    .ForContext(nameof(EdvalClassMembership), membership, true)
                    .ForContext(nameof(Error), srn.Error, true)
                    .Warning("Invalid StudentReferenceNumber provided");

                continue;
            }

            Student student = existingStudents.FirstOrDefault(student => student.StudentReferenceNumber == srn.Value);

            if (student is null)
            {
                _logger
                    .ForContext(nameof(EdvalClassMembership), membership, true)
                    .Warning("Unable to find matching student by StudentReferenceNumber");

                // Additional class enrolment in Edval
                _edvalRepository.Insert(new Difference()
                {
                    Type = EdvalDifferenceType.EdvalClassMembership,
                    Description = $"{membership.StudentId} (invalid) is not enrolled in {membership.OfferingName} in Constellation"
                });

                continue;
            }

            Offering offering = existingOfferings.FirstOrDefault(offering => offering.Name.Value == membership.OfferingName);

            if (offering is null)
            {
                _logger
                    .ForContext(nameof(EdvalClassMembership), membership, true)
                    .Warning("Unable to find matching offering by OfferingName");

                continue;
            }
            
            if (existingEnrolments.All(enrolment => enrolment.StudentId != student.Id && enrolment.OfferingId != offering.Id))
            {
                // Additional class enrolment in Edval
                _edvalRepository.Insert(new Difference()
                {
                    Type = EdvalDifferenceType.EdvalClassMembership,
                    Description = $"{student.Name} is not enrolled in {membership.OfferingName} in Constellation"
                });
            }
        }

        foreach (Enrolment enrolment in existingEnrolments)
        {
            Student student = existingStudents.FirstOrDefault(student => student.Id == enrolment.StudentId);

            if (student is null)
            {
                _logger
                    .ForContext(nameof(Enrolment), enrolment, true)
                    .Warning("Unable to find matching student by Id");

                continue;
            }

            Offering offering = existingOfferings.FirstOrDefault(offering => offering.Id == enrolment.OfferingId);

            if (offering is null)
            {
                _logger
                    .ForContext(nameof(Enrolment), enrolment, true)
                    .Warning("Unable to find matching offering by Id");

                continue;
            }

            if (edvalClassMemberships.All(entry => entry.StudentId != student.StudentReferenceNumber.Number && entry.OfferingName != offering.Name.Value))
            {
                // Additional class enrolment in Constellation
                _edvalRepository.Insert(new Difference()
                {
                    Type = EdvalDifferenceType.EdvalClassMembership,
                    Description = $"{student.Name} is not enrolled in {offering.Name} in Edval"
                });
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}