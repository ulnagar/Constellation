using Constellation.Application.DTOs.EmailRequests;
using Constellation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.API.Controllers
{
    [Route("api/v1/Emails")]
    [ApiController]
    public class EmailsController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailsController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("LessonMissedNotification")]
        public async Task SendLessonMissedEmail(LessonMissedNotificationEmail notification)
        {
            await _emailService.SendLessonMissedEmail(notification);
        }

        [HttpPost("ServiceLog")]
        public async Task SendServiceLogEmail(ServiceLogEmail notification)
        {
            await _emailService.SendServiceLogEmail(notification);
        }

        [HttpPost("AttendanceReport")]
        public async Task SendAttendanceReportEmail(AttendanceReportEmail notification)
        {
            await _emailService.SendAttendanceReport(notification);
        }

        [HttpGet("AttendanceReport")]
        public async Task SendAttendanceReportEmail()
        {
            var notification = new AttendanceReportEmail
            {
                NotificationType = AttendanceReportEmail.NotificationSequence.Student,
                StartDate = new System.DateTime(2022, 01, 17),
                EndDate = new System.DateTime(2022, 01, 17).AddDays(12),
                StudentName = "John Masters"
            };

            notification.Recipients.Add(new EmailBaseClass.Recipient { Name = "Ben", Email = "benjamin.hillsley@det.nsw.edu.au" });

            await _emailService.SendAttendanceReport(notification);
        }
    }
}
