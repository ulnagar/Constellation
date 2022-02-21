using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Identity;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Home.Pages
{
    [Authorize]
    public class DashboardModel : BasePageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardModel(IUnitOfWork unitOfWork)
            : base()
        {
            _unitOfWork = unitOfWork;
            ClassworkNotifications = new List<ClassworkNotificationDto>();
        }

        public string UserName { get; set; }

        public ICollection<ClassworkNotificationDto> ClassworkNotifications { get; set; }

        public bool IsAdmin { get; set; }

        public class ClassworkNotificationDto
        {
            public ClassworkNotificationDto()
            {
                Students = new List<string>();
            }

            public Guid Id { get; set; }
            public string ClassName { get; set; }
            public DateTime ClassDate { get; set; }
            public ICollection<string> Students { get; set; }
        }

        public async Task OnGet()
        {
            var username = User.Identity.Name;
            var teacher = await _unitOfWork.Staff.FromEmailForExistCheck(username);
            IsAdmin = User.IsInRole(AuthRoles.Admin);

            if (teacher == null) return;

            UserName = teacher.DisplayName;

            await GetClasses(_unitOfWork);

            var classworkNotifications = await _unitOfWork.ClassworkNotifications.GetOutstandingForTeacher(teacher.StaffId);

            foreach (var notification in classworkNotifications)
            {
                ClassworkNotifications.Add(new ClassworkNotificationDto
                {
                    Id = notification.Id,
                    ClassName = notification.Offering.Name,
                    ClassDate = notification.AbsenceDate,
                    Students = notification.Absences.Select(absence => absence.Student.DisplayName).ToList()
                });
            }
        }
    }
}
