using Constellation.Application.Features.Lessons.Notifications;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Infrastructure.Templates.Views.Emails.Lessons;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Lessons.Notifications.StudentMarkedPresentInScienceLesson
{
    public class SendStudentReminderEmail : INotificationHandler<StudentMarkedPresentInScienceLessonNotification>
    {
        private readonly IAppDbContext _context;
        private readonly IEmailGateway _emailSender;
        private readonly IRazorViewToStringRenderer _razorService;

        public SendStudentReminderEmail(IAppDbContext context, IEmailGateway emailSender, IRazorViewToStringRenderer razorService)
        {
            _context = context;
            _emailSender = emailSender;
            _razorService = razorService;
        }

        public async Task Handle(StudentMarkedPresentInScienceLessonNotification notification, CancellationToken cancellationToken)
        {
            // Validate entries
            var lessonRoll = await _context.LessonRolls
                .Include(roll => roll.Lesson)
                .ThenInclude(lesson => lesson.Offerings)
                .ThenInclude(offering => offering.Course)
                .Include(roll => roll.Attendance)
                .FirstOrDefaultAsync(roll => roll.Id == notification.RollId, cancellationToken);

            var student = await _context.Students
                .FirstOrDefaultAsync(student => student.StudentId == notification.StudentId, cancellationToken);

            if (lessonRoll == null || student == null)
                return;

            // Send ACC email
            var viewModel = new StudentMarkedPresentEmailViewModel
            {
                Title = $"Welcome to Aurora College!",
                SenderName = "Silvia Rudmann",
                SenderTitle = "R/Head Teacher Science and Agriculture",
                Preheader = "",
                StudentName = student.DisplayName,
                LessonTitle = lessonRoll.Lesson.Name,
                Subject = lessonRoll.Lesson.Offerings.First().Course.Name
            };

            var toRecipients = new Dictionary<string, string>
            {
                { student.DisplayName, student.EmailAddress }
            };

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Lessons/StudentMarkedPresentEmail.cshtml", viewModel);

            await _emailSender.Send(toRecipients, "noreply@aurora.nsw.edu.au", viewModel.Title, body);
        }
    }
}
