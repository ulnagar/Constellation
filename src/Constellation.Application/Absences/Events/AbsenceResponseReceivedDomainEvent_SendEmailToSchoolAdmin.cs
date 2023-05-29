namespace Constellation.Application.Absences.Events;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions;
using Constellation.Core.DomainEvents;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AbsenceResponseReceivedDomainEvent_SendEmailToSchoolAdmin
    : IDomainEventHandler<AbsenceResponseReceivedDomainEvent>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IAbsenceResponseRepository _responseRepository;
    private readonly ICourseOfferingRepository _offeringRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AbsenceResponseReceivedDomainEvent_SendEmailToSchoolAdmin(
        IAbsenceRepository absenceRepository,
        IAbsenceResponseRepository responseRepository,
        ICourseOfferingRepository offeringRepository,
        IStudentRepository studentRepository,
        IStaffRepository staffRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _responseRepository = responseRepository;
        _offeringRepository = offeringRepository;
        _studentRepository = studentRepository;
        _staffRepository = staffRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<AbsenceResponseReceivedDomainEvent>();
    }

    public async Task Handle(AbsenceResponseReceivedDomainEvent notification, CancellationToken cancellationToken)
    {
        var absence = await _absenceRepository.GetById(notification.AbsenceId, cancellationToken);

        if (absence is null)
        {
            _logger.Warning("{action}: Could not find absence with Id {id} in database", nameof(AbsenceResponseReceivedDomainEvent_SendEmailToSchoolAdmin), notification.AbsenceId);

            return;
        }

        var response = await _responseRepository.GetById(notification.ResponseId, cancellationToken);

        if (response is null)
        {
            _logger.Warning("{action}: Could not find response with Id {response_id} to absence with Id {absence_id} in database", nameof(AbsenceResponseReceivedDomainEvent_SendEmailToSchoolAdmin), notification.ResponseId, notification.AbsenceId);

            return;
        }

        var offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger.Warning("{action}: Could not find offering with Id {offering_id} when working on absence with Id {absence_id} in database", nameof(AbsenceResponseReceivedDomainEvent_SendEmailToSchoolAdmin), absence.OfferingId, notification.AbsenceId);

            return;
        }

        var student = await _studentRepository.GetById(absence.StudentId, cancellationToken);

        if (student is null)
        {
            _logger.Warning("{action}: Could not find student with Id {student_id} when working on absence with Id {absence_id} in database", nameof(AbsenceResponseReceivedDomainEvent_SendEmailToSchoolAdmin), absence.StudentId, notification.AbsenceId);

            return;
        }

        var teachers = await _staffRepository.GetPrimaryTeachersForOffering(offering.Id, cancellationToken);

        var notificationEmail = new EmailDtos.AbsenceResponseEmail();

        notificationEmail.Recipients.Add("auroracoll-h.school@det.nsw.edu.au");
        notificationEmail.Recipients.AddRange(teachers.Select(teacher => teacher.EmailAddress));
        notificationEmail.WholeAbsences.Add(new EmailDtos.AbsenceResponseEmail.AbsenceDto(absence, response, offering));
        notificationEmail.StudentName = student.DisplayName;

        await _emailService.SendAbsenceReasonToSchoolAdmin(notificationEmail);

        response.MarkForwarded();

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
