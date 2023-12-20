#nullable enable
namespace Constellation.Presentation.Server.Areas.Home.Pages;

using Application.Affirmations;
using Application.Students.CountStudentsWithAbsenceScanDisabled;
using Application.Students.CountStudentsWithAwardOverages;
using Application.Students.CountStudentsWithoutSentralId;
using Application.Students.CountStudentsWithPendingAwards;
using Application.Training.Modules.GetCountOfExpiringCertificatesForStaffMember;
using Application.Training.Roles.CountStaffWithoutRole;
using Constellation.Application.Models.Auth;
using Constellation.Application.StaffMembers.GetStaffByEmail;
using Constellation.Application.StaffMembers.Models;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class DashboardModel : BasePageModel
{
    private readonly IMediator _mediator;

    public DashboardModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public string UserName { get; set; }
    public bool IsAdmin { get; set; }
    public string StaffId { get; set; }

    public string Message { get; set; }

    public int ExpiringTraining { get; set; } = 0;

    public bool ShowTrainingWidgets { get; set; }
    public int WithoutRole { get; set; }

    public bool ShowAbsenceWidgets { get; set; }
    public int PartialScanDisabled { get; set; }
    public int WholeScanDisabled { get; set; }

    public bool ShowAwardsWidgets { get; set; }
    public int AwardOverages { get; set; }
    public int AwardAdditions { get; set; }

    public bool ShowSentralIdWidgets { get; set; }
    public int StudentsWithoutSentralId { get; set; }

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken = default)
    {
        await GetClasses(_mediator);

        string? username = User.Identity?.Name;
        bool isStaff = User.IsInRole(AuthRoles.StaffMember);
        IsAdmin = User.IsInRole(AuthRoles.Admin);
        bool isTrainingManager = User.IsInRole(AuthRoles.MandatoryTrainingEditor);
        bool isAbsencesManager = User.IsInRole(AuthRoles.AbsencesEditor);
        bool isAwardsManager = User.IsInRole(AuthRoles.AwardsManager);

        if (!isStaff && !IsAdmin)
            return RedirectToPage("Index", new { area = "" });

        Result<StaffSelectionListResponse> teacherRequest = await _mediator.Send(new GetStaffByEmailQuery(username), cancellationToken);

        Result<string> messageRequest = await _mediator.Send(new GetAffirmationQuery(teacherRequest.Value?.StaffId), cancellationToken);

        if (messageRequest.IsSuccess)
        {
            Message = messageRequest.Value;
        }

        if (teacherRequest.IsFailure)
        {
            return Page();
        }
        
        StaffId = teacherRequest.Value!.StaffId;
        UserName = $"{teacherRequest.Value.FirstName} {teacherRequest.Value.LastName}";

        Result<int> trainingExpiringSoonRequest = await _mediator.Send(new GetCountOfExpiringCertificatesForStaffMemberQuery(StaffId), cancellationToken);

        if (trainingExpiringSoonRequest.IsSuccess)
            ExpiringTraining = trainingExpiringSoonRequest.Value;

        if (isTrainingManager || IsAdmin)
        {
            ShowTrainingWidgets = true;

            Result<int> countOfStaffWithoutRoles = await _mediator.Send(new CountStaffWithoutRoleQuery(), cancellationToken);

            if (countOfStaffWithoutRoles.IsSuccess)
                WithoutRole = countOfStaffWithoutRoles.Value;
        }

        if (isAbsencesManager || IsAdmin)
        {
            ShowAbsenceWidgets = true;

            Result<(int Whole, int Partial)> absenceScanRequest = await _mediator.Send(new CountStudentsWithAbsenceScanDisabledQuery(), cancellationToken);

            if (absenceScanRequest.IsSuccess)
            {
                WholeScanDisabled = absenceScanRequest.Value.Whole;
                PartialScanDisabled = absenceScanRequest.Value.Partial;
            }
        }

        if (isAwardsManager || IsAdmin)
        {
            ShowAwardsWidgets = true;

            Result<int> overages = await _mediator.Send(new CountStudentsWithAwardOveragesQuery(), cancellationToken);

            if (overages.IsSuccess)
            {
                AwardOverages = overages.Value;
            }

            Result<int> pending = await _mediator.Send(new CountStudentsWithPendingAwardsQuery(), cancellationToken);

            if (pending.IsSuccess)
            {
                AwardAdditions = pending.Value;
            }
        }

        if (IsAdmin)
        {
            ShowSentralIdWidgets = true;

            Result<int> sentralIdRequest = await _mediator.Send(new CountStudentsWithoutSentralIdQuery(), cancellationToken);

            if (sentralIdRequest.IsSuccess)
            {
                StudentsWithoutSentralId = sentralIdRequest.Value;
            }
        }

        return Page();
    }
}
