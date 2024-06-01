namespace Constellation.Presentation.Staff.ViewComponents;

using Core.Abstractions.Services;
using Core.Models;
using Core.Models.StaffMembers.Repositories;
using Microsoft.AspNetCore.Mvc;

public class StaffSidebarMenuViewComponent : ViewComponent
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IStaffRepository _staffRepository;

    public StaffSidebarMenuViewComponent(
        ICurrentUserService currentUserService,
        IStaffRepository staffRepository)
    {
        _currentUserService = currentUserService;
        _staffRepository = staffRepository;
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
            "Equipment" => View("Equipment", activePage),
            "Partner" => View("Partner", activePage),
            "ShortTerm" => View("ShortTerm", activePage),
            "SchoolAdmin" => View("SchoolAdmin", (activePage, staffId)),
            _ => Content(string.Empty)
        };
    }
}