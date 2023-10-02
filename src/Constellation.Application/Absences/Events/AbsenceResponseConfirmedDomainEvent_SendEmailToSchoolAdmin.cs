namespace Constellation.Application.Absences.Events;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.DomainEvents;
using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AbsenceResponseConfirmedDomainEvent_SendEmailToSchoolAdmin
    : IDomainEventHandler<AbsenceResponseConfirmedDomainEvent>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AbsenceResponseConfirmedDomainEvent_SendEmailToSchoolAdmin(
        IAbsenceRepository absenceRepository,
        IOfferingRepository offeringRepository,
        IStudentRepository studentRepository,
        IStaffRepository staffRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _offeringRepository = offeringRepository;
        _studentRepository = studentRepository;
        _staffRepository = staffRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<AbsenceResponseConfirmedDomainEvent>();
    }

    public async Task Handle(AbsenceResponseConfirmedDomainEvent notification, CancellationToken cancellationToken)
    {
        Absence absence = await _absenceRepository.GetById(notification.AbsenceId, cancellationToken);

        if (absence is null)
        {
            _logger.Warning("{action}: Could not find absence with Id {id} in database", nameof(AbsenceResponseReceivedDomainEvent_SendEmailToSchoolAdmin), notification.AbsenceId);

            return;
        }

        Response response = absence.Responses.FirstOrDefault(response => response.Id == notification.ResponseId);

        if (response is null)
        {
            _logger.Warning("{action}: Could not find response with Id {response_id} to absence with Id {absence_id} in database", nameof(AbsenceResponseReceivedDomainEvent_SendEmailToSchoolAdmin), notification.ResponseId, notification.AbsenceId);

            return;
        }

        Offering offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger.Warning("{action}: Could not find offering with Id {offering_id} when working on absence with Id {absence_id} in database", nameof(AbsenceResponseReceivedDomainEvent_SendEmailToSchoolAdmin), absence.OfferingId, notification.AbsenceId);

            return;
        }

        Student student = await _studentRepository.GetById(absence.StudentId, cancellationToken);

        if (student is null)
        {
            _logger.Warning("{action}: Could not find student with Id {student_id} when working on absence with Id {absence_id} in database", nameof(AbsenceResponseReceivedDomainEvent_SendEmailToSchoolAdmin), absence.StudentId, notification.AbsenceId);

            return;
        }

        EmailDtos.AbsenceResponseEmail notificationEmail = new();

        notificationEmail.Recipients.Add("auroracoll-h.school@det.nsw.edu.au");
        notificationEmail.WholeAbsences.Add(new EmailDtos.AbsenceResponseEmail.AbsenceDto(absence, response, offering));
        notificationEmail.StudentName = student.DisplayName;

        await _emailService.SendAbsenceReasonToSchoolAdmin(notificationEmail);

        response.MarkForwarded();

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
