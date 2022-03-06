using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc;
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
        [BindProperty(SupportsGet = true)]
        public FilterDto Filter { get; set; }


        public async Task<IActionResult> OnGet()
        {
            var user = User.Identity.Name;
            var teacher = await _unitOfWork.Staff.FromEmailForExistCheck(user);

            if (teacher != null)
            {
                var notifications = await _unitOfWork.ClassworkNotifications.GetForTeacher(teacher.StaffId);

                Notifications = Filter switch
                {
                    FilterDto.Complete => notifications.Where(notification => notification.CompletedAt.HasValue).Select(notification => NotificationDto.ConvertFromNotification(notification)).ToList(),
                    FilterDto.All => notifications.Select(notification => NotificationDto.ConvertFromNotification(notification)).ToList(),
                    _ => notifications.Where(notification => !notification.CompletedAt.HasValue).Select(notification => NotificationDto.ConvertFromNotification(notification)).ToList(),
                };
            }

            await GetClasses(_unitOfWork);

            return Page();
        }

        public class NotificationDto
        {
            public NotificationDto()
            {
                Students = new List<string>();
            }

            public Guid Id { get; set; }
            public DateTime ClassDate { get; set; }
            public string ClassName { get; set; }
            public int NumStudents { get; set; }
            public ICollection<string> Students { get; set; }
            public bool IsCompleted { get; set; }

            public static NotificationDto ConvertFromNotification(ClassworkNotification notification)
            {
                var viewModel = new NotificationDto
                {
                    Id = notification.Id,
                    ClassDate = notification.AbsenceDate,
                    ClassName = notification.Offering.Name,
                    NumStudents = notification.Absences.Count,
                    Students = notification.Absences.Select(absence => absence.Student).OrderBy(student => student.LastName).Select(student => student.DisplayName).ToList(),
                    IsCompleted = notification.CompletedAt.HasValue
                };

                return viewModel;
            }
        }

        public enum FilterDto
        {
            Pending,
            Complete,
            All
        }
    }
}
