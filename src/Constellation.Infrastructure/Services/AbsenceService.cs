namespace Constellation.Infrastructure.Services;

using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using Constellation.Infrastructure.DependencyInjection;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using System;
using System.Linq;
using System.Threading.Tasks;

public class AbsenceService : IAbsenceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    
    public AbsenceService(
        IUnitOfWork unitOfWork, 
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task CreateSingleStudentExplanation(Guid absenceId, string explanation)
    {
        var absence = await _unitOfWork.Absences.ForExplanationFromStudent(absenceId);

        if (absence == null)
            return;

        var response = new AbsenceResponse()
        {
            Absence = absence,
            AbsenceId = absence.Id,
            Type = AbsenceResponse.Student,
            From = AbsenceResponse.Student,
            ReceivedAt = DateTime.Now,
            Explanation = explanation,
            VerificationStatus = AbsenceResponse.Pending,
            VerifiedAt = DateTime.Now
        };

        absence.Responses.Add(response);
        await _unitOfWork.CompleteAsync();

        var notificationEmail = new EmailDtos.AbsenceResponseEmail();

        var contacts = absence.Student
            .School
            .StaffAssignments
            .Where(assignment => !assignment.IsDeleted && !assignment.SchoolContact.IsDeleted && assignment.Role == SchoolContactRole.Coordinator)
            .Select(assignment => assignment.SchoolContact.EmailAddress)
            .ToList();

        notificationEmail.Recipients.AddRange(contacts);
        notificationEmail.PartialAbsences.Add(absence);

        var email = await _emailService.SendCoordinatorPartialAbsenceVerificationRequest(notificationEmail);
        response.Forwarded = true;

        var notification = new AbsenceNotification
        {
            Absence = absence,
            AbsenceId = absence.Id,
            OutgoingId = email.id,
            Type = "Email",
            Message = email.message,
            Recipients = email.recipients,
            SentAt = DateTime.Now,
        };

        absence.Notifications.Add(notification);

        await _unitOfWork.CompleteAsync();
    }
}