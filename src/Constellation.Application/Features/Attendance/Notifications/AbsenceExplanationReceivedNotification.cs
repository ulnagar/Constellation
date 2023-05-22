namespace Constellation.Application.Features.Attendance.Notifications;

using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models.Absences;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class AbsenceExplanationReceivedNotification : INotification
{
    public Guid AbsenceResponseId { get; set; }
}

public class SendAbsenceExplanationToAdmin : INotificationHandler<AbsenceExplanationReceivedNotification>
{
    private readonly IAppDbContext _context;
    private readonly IEmailService _emailService;

    public SendAbsenceExplanationToAdmin(IAppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task Handle(AbsenceExplanationReceivedNotification notification, CancellationToken cancellationToken)
    {
        var absence = await _context.Absences
            .Include(absence => absence.Responses)
            .Include(absence => absence.Offering)
            .FirstOrDefaultAsync(absence => absence.Responses.Any(response => response.Id == notification.AbsenceResponseId), cancellationToken);

        var response = absence.Responses.First(response => response.Id == notification.AbsenceResponseId);

        if (absence.Type != Absence.Whole && response.VerificationStatus != AbsenceResponse.Verified)
            return;

        var student = await _context.Students
            .FirstOrDefaultAsync(student => student.StudentId == absence.StudentId, cancellationToken);

        var teachers = await _context.Sessions
            .Where(session => !session.IsDeleted && session.OfferingId == absence.OfferingId)
            .Select(session => session.Teacher.EmailAddress)
            .Distinct()
            .ToListAsync(cancellationToken);
        
        var notificationEmail = new EmailDtos.AbsenceResponseEmail();

        notificationEmail.Recipients.Add("auroracoll-h.school@det.nsw.edu.au");
        notificationEmail.Recipients.AddRange(teachers);
        notificationEmail.WholeAbsences.Add(new EmailDtos.AbsenceResponseEmail.AbsenceDto(absence, response));
        notificationEmail.StudentName = student.DisplayName;

        await _emailService.SendAbsenceReasonToSchoolAdmin(notificationEmail);

        response.Forwarded = true;
        await _context.SaveChangesAsync(cancellationToken);
    }
}