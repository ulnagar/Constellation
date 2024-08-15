namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Absences;

using Application.Common.PresentationModels;
using Constellation.Application.Models.Auth;
using Constellation.Application.Students.GetStudentsWithAbsenceSettings;
using Constellation.Core.Models.Absences;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class AuditModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public AuditModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<AuditModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Absences_Audit;
    [ViewData] public string PageTitle => "Student Absence Settings";
    
    [BindProperty(SupportsGet = true)]
    public FilterDto Filter { get; set; } = FilterDto.All;

    public List<StudentAbsenceSettingsResponse> Students { get; set; } = new();

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve student absence settings by user {User}", _currentUserService.UserName);

        Result<List<StudentAbsenceSettingsResponse>> studentRequest = await _mediator.Send(new GetStudentsWithAbsenceSettingsQuery());

        if (studentRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), studentRequest.Error, true)
                .Warning("Failed to retrieve student absence settings by user {User}", _currentUserService.UserName);
            
            ModalContent = new ErrorDisplay(
                studentRequest.Error,
                _linkGenerator.GetPathByPage("/Partner/Students/Index", values: new { area = "Staff" }));

            return;
        }
        
        Students = studentRequest
            .Value
            .OrderBy(student => student.School)
            .ThenBy(student => student.Grade)
            .ThenBy(student => student.Name)
            .ToList();

        if (Filter == FilterDto.Disabled)
        {
            Students = Students
                .Where(student =>
                {
                    bool anyWholeAbsences = student.AbsenceSettings.Any(entry => entry.AbsenceType == AbsenceType.Whole);
                    bool anyPartialAbsences = student.AbsenceSettings.Any(entry => entry.AbsenceType == AbsenceType.Partial);

                    return (!anyWholeAbsences || !anyPartialAbsences);
                })
                .ToList();
        }

        if (Filter == FilterDto.Enabled)
        {
            Students = Students
                .Where(student =>
                {
                    bool anyWholeAbsences = student.AbsenceSettings.Any(entry => entry.AbsenceType == AbsenceType.Whole);
                    bool anyPartialAbsences = student.AbsenceSettings.Any(entry => entry.AbsenceType == AbsenceType.Partial);

                    return (anyWholeAbsences || anyPartialAbsences);
                })
                .ToList();
        }
    }

    public enum FilterDto
    {
        All,
        Enabled,
        Disabled
    }
}
