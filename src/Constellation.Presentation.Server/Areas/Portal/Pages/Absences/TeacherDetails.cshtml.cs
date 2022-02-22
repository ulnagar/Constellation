using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Portal.Pages.Absences
{
    public class TeacherDetailsModel : BasePageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public TeacherDetailsModel(IUnitOfWork unitOfWork)
            : base()
        {
            _unitOfWork = unitOfWork;
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }
        public NotificationDto Notification { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var entry = await _unitOfWork.ClassworkNotifications.Get(Id);

            Notification = NotificationDto.ConvertFromNotification(entry);

            await GetClasses(_unitOfWork);

            return Page();
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
