using Constellation.Application.DTOs.EmailRequests;
using Constellation.Application.Features.Jobs.AbsenceMonitor.Queries;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Jobs
{
    public class AbsenceClassworkNotificationJob : IAbsenceClassworkNotificationJob, IScopedService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ISentralGateway _sentralService;
        private readonly ILogger<IAbsenceMonitorJob> _logger;
        private readonly IMediator _mediator;

        public AbsenceClassworkNotificationJob(IUnitOfWork unitOfWork, IEmailService emailService,
            ISentralGateway sentralService, ILogger<IAbsenceMonitorJob> logger,
            IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _sentralService = sentralService;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task StartJob(DateTime scanDate)
        {
            _logger.LogInformation("Starting missed classwork notifications scan:");

            var absences = await _unitOfWork.Absences.ForClassworkNotifications(scanDate);

            var absencesByClassAndDate = absences.GroupBy(absence => new { absence.OfferingId, absence.Date.Date });

            foreach (var group in absencesByClassAndDate)
            {
                var absenceDate = group.Key.Date;
                var offeringId = group.Key.OfferingId;

                // Make sure to filter out cancelled sessions and deleted teachers
                var teachers = group.First().Offering.Sessions.Where(session => !session.IsDeleted).Select(session => session.Teacher).Where(teacher => !teacher.IsDeleted).Distinct().ToList();
                var students = group.Select(absence => absence.Student).Distinct().ToList();
                var offering = group.First().Offering;

                var casualCovers = await _unitOfWork.CasualClassCovers.ForClassworkNotifications(absenceDate, offeringId);
                var teacherCovers = await _unitOfWork.TeacherClassCovers.ForClassworkNotifications(absenceDate, offeringId);

                var covers = new List<ClassCover>();
                foreach (var cover in casualCovers)
                    covers.Add(cover);
                foreach (var cover in teacherCovers)
                    covers.Add(cover);

                if (covers.Count > 0)
                    teachers = new List<Staff> { offering.Course.HeadTeacher };

                // create notification object in database
                var notification = new ClassworkNotification
                {
                    AbsenceDate = absenceDate,
                    Absences = group.ToList(),
                    Covers = covers,
                    Offering = offering,
                    OfferingId = offeringId,
                    Teachers = teachers
                };

                // Check if the db already has an entry for this?
                var check = await _unitOfWork.ClassworkNotifications.GetForDuplicateCheck(offeringId, absenceDate);
                if (check == null)
                {
                    _unitOfWork.Add(notification);
                    await _unitOfWork.CompleteAsync();

                    _logger.LogInformation(" Sending email for {Offering} @ {AbsenceDate}", notification.Offering.Name, notification.AbsenceDate.ToShortDateString());
                    foreach (var teacher in teachers)
                        _logger.LogInformation($"  {teacher.DisplayName} - {teacher.EmailAddress}");

                    var emailNotification = new ClassworkNotificationTeacherEmail
                    {
                        NotificationId = notification.Id,
                        OfferingName = notification.Offering.Name,
                        Students = notification.Absences.Select(absence => absence.Student.DisplayName).ToList(),
                        AbsenceDate = notification.AbsenceDate,
                        Teachers = notification.Teachers.Select(teacher => new ClassworkNotificationTeacherEmail.Teacher { Name = teacher.DisplayName, Email = teacher.EmailAddress}).ToList(),
                        IsCovered = notification.Covers.Any()
                    };

                    await _emailService.SendTeacherClassworkNotificationRequest(emailNotification);
                }
                else
                {
                    // Somehow, and this should not happen, there is already an entry for this occurance?
                    // Compare the list of students to see who has been added (or removed) and update the database
                    // entry accordingly. Then, if the teacher has already responded, send the student/parent email
                    // with the details of work required.

                    _logger.LogInformation(" Found existing entry for {Offering} @ {AbsenceDate}", notification.Offering.Name, notification.AbsenceDate.ToShortDateString());

                    var newAbsences = new List<Absence>();

                    foreach (var absence in notification.Absences)
                    {
                        if (!check.Absences.Contains(absence))
                        {
                            newAbsences.Add(absence);

                            _logger.LogInformation("  Adding {Student} to the existing entry", absence.Student.DisplayName);
                        }
                    }

                    if (check.CompletedBy != null)
                    {
                        // Teacher has already submitted response. Add new students and send their email directly
                        foreach (var absence in newAbsences)
                        {
                            check.Absences.Add(absence);

                            var parentEmails = await _mediator.Send(new GetStudentFamilyEmailAddressesQuery { StudentId = absence.StudentId });
                            await _emailService.SendStudentClassworkNotification(absence, check, parentEmails.ToList());
                        }
                    }
                    else
                    {
                        foreach (var absence in newAbsences)
                            check.Absences.Add(absence);
                    }

                    await _unitOfWork.CompleteAsync();
                }
            }
        }
    }
}
