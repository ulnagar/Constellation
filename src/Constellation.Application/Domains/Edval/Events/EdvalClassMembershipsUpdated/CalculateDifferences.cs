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
using Core.Models.Tutorials;
using Core.Models.Tutorials.Repositories;
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
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CalculateDifferences(
        IEdvalRepository edvalRepository,
        IOfferingRepository offeringRepository,
        IStudentRepository studentRepository,
        IEnrolmentRepository enrolmentRepository,
        ITutorialRepository tutorialRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _edvalRepository = edvalRepository;
        _offeringRepository = offeringRepository;
        _enrolmentRepository = enrolmentRepository;
        _tutorialRepository = tutorialRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<EdvalClassMembershipsUpdatedIntegrationEvent>();
    }
    public async Task Handle(EdvalClassMembershipsUpdatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        List<Student> existingStudents = await _studentRepository.GetCurrentStudents(cancellationToken);
        List<Offering> existingOfferings = await _offeringRepository.GetAllActive(cancellationToken);
        List<Tutorial> existingTutorials = await _tutorialRepository.GetAllActive(cancellationToken);
        List<Enrolment> existingEnrolments = await _enrolmentRepository.GetCurrent(cancellationToken);

        List<EdvalClassMembership> edvalClassMemberships = await _edvalRepository.GetClassMemberships(cancellationToken);
        List<EdvalIgnore> ignoredMemberships = await _edvalRepository.GetIgnoreRecords(EdvalDifferenceType.EdvalClassMembership, cancellationToken);

        foreach (EdvalClassMembership membership in edvalClassMemberships)
        {
            bool ignored = ignoredMemberships
                .Where(ignore => ignore.System == EdvalDifferenceSystem.EdvalDifference)
                .Any(ignore => ignore.Identifier == membership.Identifier);

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
                _edvalRepository.Insert(new Difference(
                    EdvalDifferenceType.EdvalClassMembership,
                    EdvalDifferenceSystem.EdvalDifference, 
                    membership.Identifier,
                    $"{membership.StudentId} (invalid) is not enrolled in {membership.OfferingName} in Constellation",
                    ignored));

                continue;
            }

            Offering offering = existingOfferings.FirstOrDefault(offering => offering.Name.Value == membership.OfferingName);
            Tutorial tutorial = existingTutorials.FirstOrDefault(tutorial => tutorial.Name.Value == membership.OfferingName);

            if (offering is null && tutorial is null)
            {
                _logger
                    .ForContext(nameof(EdvalClassMembership), membership, true)
                    .Warning("Unable to find matching offering by OfferingName");

                continue;
            }

            bool offeringEnrolments = existingEnrolments
                .OfType<OfferingEnrolment>()
                .All(enrolment =>
                    enrolment.StudentId != student.Id && 
                    enrolment.OfferingId != offering?.Id);

            bool tutorialEnrolments = existingEnrolments
                .OfType<TutorialEnrolment>()
                .All(enrolment =>
                    enrolment.StudentId != student.Id &&
                    enrolment.TutorialId != tutorial?.Id);
            
            if (!offeringEnrolments && !tutorialEnrolments)
            {
                // Additional class enrolment in Edval
                _edvalRepository.Insert(new Difference(
                    EdvalDifferenceType.EdvalClassMembership,
                    EdvalDifferenceSystem.EdvalDifference, 
                    membership.Identifier,
                    $"{student.Name} is not enrolled in {membership.OfferingName} in Constellation",
                    ignored));
            }
        }

        foreach (Enrolment enrolment in existingEnrolments)
        {
            bool ignored = ignoredMemberships
                .Where(ignore => ignore.System == EdvalDifferenceSystem.ConstellationDifference)
                .Any(ignore => ignore.Identifier == enrolment.Id.ToString());

            Student student = existingStudents.FirstOrDefault(student => student.Id == enrolment.StudentId);

            if (student is null)
            {
                _logger
                    .ForContext(nameof(Enrolment), enrolment, true)
                    .Warning("Unable to find matching student by Id");

                continue;
            }

            if (enrolment is OfferingEnrolment offeringEnrolment)
            {
                Offering offering = existingOfferings.FirstOrDefault(offering => offering.Id == offeringEnrolment.OfferingId);

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
                    _edvalRepository.Insert(new Difference(
                        EdvalDifferenceType.EdvalClassMembership,
                        EdvalDifferenceSystem.ConstellationDifference,
                        enrolment.Id.ToString(),
                        $"{student.Name} is not enrolled in {offering.Name} in Edval",
                        ignored));
                }

                continue;
            }

            if (enrolment is TutorialEnrolment tutorialEnrolment)
            {
                Tutorial tutorial = existingTutorials.FirstOrDefault(tutorial => tutorial.Id == tutorialEnrolment.TutorialId);

                if (tutorial is null)
                {
                    _logger
                        .ForContext(nameof(Enrolment), enrolment, true)
                        .Warning("Unable to find matching tutorial by Id");

                    continue;
                }

                if (edvalClassMemberships.All(entry => entry.StudentId != student.StudentReferenceNumber.Number && entry.OfferingName != tutorial.Name.Value))
                {
                    // Additional class enrolment in Constellation
                    _edvalRepository.Insert(new Difference(
                        EdvalDifferenceType.EdvalClassMembership,
                        EdvalDifferenceSystem.ConstellationDifference,
                        enrolment.Id.ToString(),
                        $"{student.Name} is not enrolled in {tutorial.Name} in Edval",
                        ignored));
                }

                continue;
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}