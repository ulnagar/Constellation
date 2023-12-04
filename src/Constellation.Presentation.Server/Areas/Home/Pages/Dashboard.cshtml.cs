﻿namespace Constellation.Presentation.Server.Areas.Home.Pages;

using Application.Affirmations;
using Constellation.Application.MandatoryTraining.GetCountOfExpiringCertificatesForStaffMember;
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

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken = default)
    {
        await GetClasses(_mediator);

        string? username = User.Identity.Name;
        bool IsStaff = User.IsInRole(AuthRoles.StaffMember);
        IsAdmin = User.IsInRole(AuthRoles.Admin);

        if (!IsStaff && !IsAdmin)
            return RedirectToPage("Index", new { area = "" });

        Result<StaffSelectionListResponse> teacherRequest = await _mediator.Send(new GetStaffByEmailQuery(username), cancellationToken);

        Result<string> messageRequest = await _mediator.Send(new GetAffirmationQuery(teacherRequest?.Value.StaffId), cancellationToken);

        if (messageRequest.IsSuccess)
        {
            Message = messageRequest.Value;
        }

        if (teacherRequest.IsFailure)
        {
            return Page();
        }

        StaffId = teacherRequest.Value.StaffId;
        UserName = $"{teacherRequest.Value.FirstName} {teacherRequest.Value.LastName}";

        Result<int> trainingExpiringSoonRequest = await _mediator.Send(new GetCountOfExpiringCertificatesForStaffMemberQuery(StaffId), cancellationToken);

        if (trainingExpiringSoonRequest.IsSuccess)
            ExpiringTraining = trainingExpiringSoonRequest.Value;

        return Page();
    }
}
