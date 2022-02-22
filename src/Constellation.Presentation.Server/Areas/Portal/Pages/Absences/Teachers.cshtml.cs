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
    public class TeachersModel : BasePageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public TeachersModel(IUnitOfWork unitOfWork)
            : base()
        {
            _unitOfWork = unitOfWork;

            Notifications = new List<NotificationDto>();
        }

        public ICollection<NotificationDto> Notifications { get; set; }


        public async Task<IActionResult> OnGet()
        {
            var user = User.Identity.Name;
            var teacher = await _unitOfWork.Staff.FromEmailForExistCheck(user);

            if (teacher != null)
            {
                var notifications = await _unitOfWork.ClassworkNotifications.GetForTeacher(teacher.StaffId);
                Notifications = notifications.Select(notification => NotificationDto.ConvertFromNotification(notification)).ToList();
            }

            await GetClasses(_unitOfWork);

            return Page();
        }

        public class NotificationDto
        {
            public Guid Id { get; set; }
            public DateTime ClassDate { get; set; }
            public string ClassName { get; set; }
            public int NumStudents { get; set; }
            public bool IsCompleted { get; set; }

            public static NotificationDto ConvertFromNotification(ClassworkNotification notification)
            {
                var viewModel = new NotificationDto
                {
                    Id = notification.Id,
                    ClassDate = notification.AbsenceDate,
                    ClassName = notification.Offering.Name,
                    NumStudents = notification.Absences.Count,
                    IsCompleted = notification.CompletedAt.HasValue
                };

                return viewModel;
            }
        }
    }
}
