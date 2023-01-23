﻿using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Features.ShortTerm.Covers.Notifications;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Constellation.Core.Abstractions;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Infrastructure.Templates.Views.Documents.Covers;
using Constellation.Infrastructure.Templates.Views.Emails.Covers;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.ShortTerm.Covers.Notifications.CasualCoverCreated
{
    public class SendCoverNotificationEmail : INotificationHandler<CasualCoverCreatedNotification>
    {
        private readonly IAppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IRazorViewToStringRenderer _razorService;
        private readonly IPDFService _pdfService;
        private readonly IEmailGateway _emailGateway;
        private readonly ICalendarService _calendarService;
        private readonly ITeamRepository _teamRepository;
        private readonly ILogger _logger;

        public SendCoverNotificationEmail(IAppDbContext context, UserManager<AppUser> userManager,
            IRazorViewToStringRenderer razorService, IPDFService pdfService, IEmailGateway emailGateway,
            ICalendarService calendarService, ILogger logger, ITeamRepository teamRepository)
        {
            _context = context;
            _userManager = userManager;
            _razorService = razorService;
            _pdfService = pdfService;
            _emailGateway = emailGateway;
            _calendarService = calendarService;
            _teamRepository = teamRepository;
            _logger = logger.ForContext<SendCoverNotificationEmail>();
        }

        public async Task Handle(CasualCoverCreatedNotification notification, CancellationToken cancellationToken)
        {
            // Gather details
            var cover = await _context.Covers
                .FirstOrDefaultAsync(cover => cover.Id == notification.CoverId, cancellationToken) as CasualClassCover;

            if (cover is null)
                return;

            var offering = await _context.Offerings
                .Include(offering => offering.Course)
                .FirstOrDefaultAsync(offering => offering.Id == cover.OfferingId, cancellationToken);

            var casual = await _context.Casuals
                .FirstOrDefaultAsync(casual => casual.Id == cover.CasualId, cancellationToken);

            var teachers = await _context.Sessions
                .Where(session => !session.IsDeleted && session.OfferingId == cover.OfferingId)
                .Select(session => session.Teacher)
                .Distinct()
                .ToListAsync(cancellationToken);

            var headTeachers = await _context.Faculties
                .Where(faculty => faculty.Id == offering.Course.FacultyId)
                .SelectMany(faculty => faculty.Members)
                .Where(member => member.Role == FacultyMembershipRole.Manager && !member.IsDeleted)
                .Select(member => member.Staff)
                .ToListAsync(cancellationToken);

            var additionalRecipients = await _userManager.GetUsersInRoleAsync(AuthRoles.CoverRecipient);

            var teamLink = await _teamRepository.GetLinkByOffering(offering.Name, offering.EndDate.Year.ToString(), cancellationToken);
                
            if (string.IsNullOrWhiteSpace(teamLink))
            {
                _logger.Warning("Could not find team for {class} while attempting to send email for cover {id}", offering.Name, cover.Id);
                teamLink = "https://teams.microsoft.com";
            }

            var primaryRecipients = new Dictionary<string, string>(); // Casual, Classroom Teacher
            var secondaryRecipients = new Dictionary<string, string>(); // Head Teacher, Additional Recipients

            primaryRecipients.Add(casual.DisplayName, casual.EmailAddress);

            foreach (var teacher in teachers)
            {
                if (!primaryRecipients.Any(recipient => recipient.Value == teacher.EmailAddress) && !secondaryRecipients.Any(recipient => recipient.Value == teacher.EmailAddress))
                    secondaryRecipients.Add(teacher.DisplayName, teacher.EmailAddress);
            }

            foreach (var teacher in headTeachers)
            {
                if (!primaryRecipients.Any(recipient => recipient.Value == teacher.EmailAddress) && !secondaryRecipients.Any(recipient => recipient.Value == teacher.EmailAddress))
                    secondaryRecipients.Add(teacher.DisplayName, teacher.EmailAddress);
            }

            foreach (var user in additionalRecipients)
            {
                if (!primaryRecipients.Any(recipient => recipient.Value == user.Email) && !secondaryRecipients.Any(recipient => recipient.Value == user.Email))
                    secondaryRecipients.Add(user.DisplayName, user.Email);
            }

            // Determine whether email or invite
            var singleDayCover = cover.StartDate == cover.EndDate;

            // Prepare attachments

            // Class Roll
            var attachments = new List<Attachment>();

            var model = new CoverRollViewModel();
            model.ClassName = offering.Name;

            var classStudents = await _context.Enrolments
                .Include(enrolment => enrolment.Student.School)
                .Where(enrolment => enrolment.OfferingId == offering.Id && !enrolment.IsDeleted)
                .Select(enrolment => enrolment.Student)
                .ToListAsync(cancellationToken);

            foreach (var student in classStudents)
            {
                var entry = new CoverRollViewModel.EnrolledStudent();
                entry.Name = student.DisplayName;
                entry.Gender = student.Gender;
                entry.School = student.School.Name;
                entry.OrderName = $"{student.LastName} {student.FirstName}";

                model.Students.Add(entry);
            }

            var rollString = await _razorService.RenderViewToStringAsync("/Views/Documents/Covers/CoverRoll.cshtml", model);
            var rollAttachment = _pdfService.StringToPdfAttachment(rollString, $"{offering.Name} Roll.pdf");

            attachments.Add(rollAttachment);

            // Timetable
            if (!singleDayCover)
            {
                var timetableData = new ClassTimetableDataDto();
                timetableData.ClassName = offering.Name;

                var sessions = await _context.Sessions
                    .Include(session => session.Period)
                    .Include(session => session.Teacher)
                    .Include(session => session.Offering)
                    .Where(session => session.OfferingId == cover.OfferingId && !session.IsDeleted)
                    .ToListAsync(cancellationToken);

                var periods = await _context.Periods.ToListAsync(cancellationToken);

                var relevantTimetables = sessions.Select(session => session.Period.Timetable).Distinct().ToList();

                var relevantPeriods = periods.Where(period => relevantTimetables.Contains(period.Timetable)).ToList();

                foreach (var period in relevantPeriods)
                {
                    if (period.Type == "Other")
                        continue;

                    var entry = new TimetableDataDto.TimetableData
                    {
                        Day = period.Day,
                        StartTime = period.StartTime,
                        EndTime = period.EndTime,
                        TimetableName = period.Timetable,
                        Name = period.Name,
                        Period = period.Period,
                        Type = period.Type
                    };

                    if (sessions.Any(session => session.PeriodId == period.Id))
                    {
                        var relevantSession = sessions.FirstOrDefault(session => session.PeriodId == period.Id);
                        entry.ClassName = relevantSession.Offering.Name;
                        entry.ClassTeacher = relevantSession.Teacher.DisplayName;
                    }

                    timetableData.Timetables.Add(entry);
                }

                var timetableHeaderString = await _razorService.RenderViewToStringAsync("/Views/Documents/Timetable/ClassTimetableHeader.cshtml", timetableData);
                var timetableBodyString = await _razorService.RenderViewToStringAsync("/Views/Documents/Timetable/Timetable.cshtml", timetableData);

                var timetableAttachment = _pdfService.StringToPdfAttachment(timetableBodyString, timetableHeaderString, $"{offering.Name} Timetable.pdf");

                attachments.Add(timetableAttachment);
            }

            // Send
            var viewModel = new NewCoverEmailViewModel
            {
                ToName = casual.DisplayName,
                Title = $"Aurora Class Cover - {offering.Name}",
                SenderName = "Cathy Crouch",
                SenderTitle = "Casual Coordinator",
                StartDate = cover.StartDate,
                EndDate = cover.EndDate,
                HasAdobeAccount = true,
                Preheader = "",
                ClassWithLink = new Dictionary<string, string> { { "Class Team", teamLink } }
            };

            if (singleDayCover)
            {
                var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Covers/NewCoverAppointment.cshtml", viewModel);

                // Create and add ICS files
                var uid = $"{cover.Id}-{cover.Offering.Id}-{cover.StartDate:yyyyMMdd}";
                var summary = $"Aurora College Cover - {cover.Offering.Name}";
                var location = $"Class Team ({teamLink})";
                var description = body;

                // What cycle day does the cover fall on?
                // What periods exist for this class on that cycle day?
                // Extract start and end times for the periods to use in the appointment
                var cycleDay = cover.StartDate.GetDayNumber();
                var periods = await _context.Periods
                    .Where(period => period.Day == cycleDay && period.OfferingSessions.Any(session => !session.IsDeleted && session.OfferingId == cover.OfferingId) && period.Type != "Other")
                    .ToListAsync(cancellationToken);

                var appointmentStartTime = periods.Min(period => period.StartTime);
                var appointmentEndTime = periods.Max(period => period.EndTime);

                var appointmentStart = cover.StartDate.Add(appointmentStartTime);
                var appointmentEnd = cover.EndDate.Add(appointmentEndTime);

                var icsData = _calendarService.CreateInvite(uid, casual.DisplayName, casual.EmailAddress, summary, location, description, appointmentStart, appointmentEnd, 0);

                await _emailGateway.Send(primaryRecipients, secondaryRecipients, "auroracoll-h.school@det.nsw.edu.au", viewModel.Title, body, attachments, icsData);
            } else
            {
                var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Covers/NewCoverEmail.cshtml", viewModel);

                await _emailGateway.Send(primaryRecipients, secondaryRecipients, "auroracoll-h.school@det.nsw.edu.au", viewModel.Title, body, attachments);
            }
        }
    }
}
