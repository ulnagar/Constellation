using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Portal.Pages.Absences
{
    public class TeacherModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISentralGateway _sentralService;
        private readonly IEmailService _emailService;

        public TeacherModel(IUnitOfWork unitOfWork, ISentralGateway sentralService, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _sentralService = sentralService;
            _emailService = emailService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }
        public NotificationDto Notification { get; set; }
        [BindProperty]
        [Required]
        public string Description { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var entry = await _unitOfWork.ClassworkNotifications.Get(Id);

            Notification = NotificationDto.ConvertFromNotification(entry);

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var entry = await _unitOfWork.ClassworkNotifications.Get(Id);

            if (!ModelState.IsValid)
            {
                Notification = NotificationDto.ConvertFromNotification(entry);

                return Page();
            }

            var user = User.Identity.Name;
            var teacher = await _unitOfWork.Staff.FromEmailForExistCheck(user);

            entry.CompletedAt = DateTime.Now;
            entry.CompletedBy = teacher;
            entry.StaffId = teacher.StaffId;
            entry.Description = Notification.Description;

            foreach (var absence in entry.Absences)
            {
                // Email student and parents with information
                var parentEmails = await _sentralService.GetContactEmailsAsync(absence.Student.SentralStudentId);
                await _emailService.SendStudentClassworkNotification(absence, entry, parentEmails);
            }

            await _unitOfWork.CompleteAsync();

            return RedirectToPage("/Index", new { area = ""});
        }

        public class NotificationDto
        {
            public Guid Id { get; set; }
            public string Description { get; set; }
            public Staff CompletedBy { get; set; }
            public string StaffId { get; set; }
            public DateTime? CompletedAt { get; set; }
            public DateTime GeneratedAt { get; set; }
            public ICollection<Student> Students { get; set; }
            public ICollection<Staff> Teachers { get; set; }
            public string OfferingName { get; set; }
            public DateTime AbsenceDate { get; set; }

            public static NotificationDto ConvertFromNotification(ClassworkNotification entry)
            {
                var viewModel = new NotificationDto
                {
                    Id = entry.Id,
                    Description = entry.Description,
                    CompletedBy = entry.CompletedBy,
                    CompletedAt = entry.CompletedAt,
                    StaffId = entry.StaffId,
                    GeneratedAt = entry.GeneratedAt,
                    Teachers = entry.Teachers,
                    AbsenceDate = entry.AbsenceDate,
                    OfferingName = entry.Offering.Name,
                    Students = entry.Absences.Select(absence => absence.Student).ToList()
                };

                return viewModel;
            }
        }
    }
}