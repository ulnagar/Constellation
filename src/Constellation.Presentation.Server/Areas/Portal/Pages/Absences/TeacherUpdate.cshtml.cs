namespace Constellation.Presentation.Server.Areas.Portal.Pages.Absences;

using Constellation.Application.Families.GetResidentialFamilyEmailAddresses;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models;
using Constellation.Core.Models.MissedWork;
using Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

[Roles(AuthRoles.Admin, AuthRoles.StaffMember)]
public class TeacherUpdateModel : BasePageModel
{
    private readonly IMediator _mediator;

    public TeacherUpdateModel(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public NotificationDto Notification { get; set; }

    [BindProperty]
    [Required]
    public string Description { get; set; }

    public async Task OnGet(CancellationToken cancellationToken = default)
    {
        await GetClasses(_mediator);

        var entry = await _unitOfWork.ClassworkNotifications.Get(Id);

        Notification = NotificationDto.ConvertFromNotification(entry);
    }

    public async Task<IActionResult> OnPost(CancellationToken cancellationToken)
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
        entry.Description = Description;

        await _unitOfWork.CompleteAsync();

        foreach (var absence in entry.Absences)
        {
            // Email student and parents with information
            var parentEmails = await _mediator.Send(new GetResidentialFamilyEmailAddressesQuery(absence.Student.StudentId), cancellationToken);

            if (parentEmails.IsFailure || !parentEmails.Value.Any())
                await _emailService.SendAdminClassworkNotificationContactAlert(absence.Student, teacher, entry);

            await _emailService.SendStudentClassworkNotification(absence, entry, parentEmails.Value);
        }

        await _emailService.SendTeacherClassworkNotificationCopy(entry.Absences.First(), entry, teacher);

        return RedirectToPage("Teachers", new { area = "Portal"});
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
