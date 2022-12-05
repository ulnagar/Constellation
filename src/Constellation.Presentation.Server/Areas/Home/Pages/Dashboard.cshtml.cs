using Constellation.Application.Features.Home.Commands;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Constellation.Presentation.Server.Areas.Home.Pages
{
    [Authorize]
    public class DashboardModel : BasePageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;

        public DashboardModel(IUnitOfWork unitOfWork, IMediator mediator)
            : base()
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
            ClassworkNotifications = new List<ClassworkNotificationDto>();
        }

        public string UserName { get; set; }

        public ICollection<ClassworkNotificationDto> ClassworkNotifications { get; set; }

        public bool IsAdmin { get; set; }

        public string Message { get; set; }

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

        public async Task<IActionResult> OnGet()
        {
            Message = await _mediator.Send(new GeneratePepTalkCommand());

            var username = User.Identity.Name;
            var IsStaff = User.IsInRole(AuthRoles.StaffMember);
            IsAdmin = User.IsInRole(AuthRoles.Admin);

            if (!IsStaff && !IsAdmin)
            {
                return RedirectToPage("Index", new { area = "" });
            }

            var teacher = await _unitOfWork.Staff.FromEmailForExistCheck(username);

            if (teacher == null)
            {
                UserName = username;
                return Page();
            }

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

            return Page();
        }
    }
}
