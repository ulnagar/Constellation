namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.StaffSidebarMenu;

using Constellation.Application.Offerings.GetCurrentOfferingsForTeacher;
using Constellation.Application.StaffMembers.GetStaffByEmail;
using Constellation.Application.StaffMembers.Models;
using Constellation.Application.Training.GetCountOfExpiringCertificatesForStaffMember;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class StaffSidebarMenuViewComponent : ViewComponent
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IStaffRepository _staffRepository;
    private readonly ISender _mediator;

    public StaffSidebarMenuViewComponent(
        ICurrentUserService currentUserService,
        IStaffRepository staffRepository,
        ISender mediator)
    {
        _currentUserService = currentUserService;
        _staffRepository = staffRepository;
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(string activePage)
    {
        string? staffEmailAddress = _currentUserService.EmailAddress;
        Staff? staffMember = staffEmailAddress is not null ? await _staffRepository.GetCurrentByEmailAddress(staffEmailAddress) : null;
        string staffId = staffMember?.StaffId ?? string.Empty;

        string[] segments = activePage.Split('.');
        string module = segments[0];
        
        return module switch
        {
            "Dashboard" => View("Dashboard", await GenerateDashboardData()),
            "Equipment" => View("Equipment", activePage),
            "Partner" => View("Partner", activePage),
            "ShortTerm" => View("ShortTerm", activePage),
            "SchoolAdmin" => View("SchoolAdmin", (activePage, staffId)),
            "StudentAdmin" => View("StudentAdmin", activePage),
            "Subject" => View("Subject", activePage),
            _ => Content(string.Empty)
        };
    }

    private async Task<DashboardModel> GenerateDashboardData()
    {
        DashboardModel model = new();

        string? username = User.Identity?.Name;

        if (username is not null)
        {
            Result<List<TeacherOfferingResponse>> query = await _mediator.Send(new GetCurrentOfferingsForTeacherQuery(null, username));

            if (query.IsSuccess)
                model.Classes = query.Value.ToDictionary(k => k.OfferingName.Value, k => k.OfferingId);
        }

        Result<StaffSelectionListResponse> teacherRequest = await _mediator.Send(new GetStaffByEmailQuery(username));
        if (teacherRequest.IsFailure)
            return model;

        model.StaffId = teacherRequest.Value!.StaffId;

        Result<int> trainingExpiringSoonRequest = await _mediator.Send(new GetCountOfExpiringCertificatesForStaffMemberQuery(model.StaffId));

        if (trainingExpiringSoonRequest.IsSuccess)
            model.ExpiringTraining = trainingExpiringSoonRequest.Value;

        return model;
    }
}