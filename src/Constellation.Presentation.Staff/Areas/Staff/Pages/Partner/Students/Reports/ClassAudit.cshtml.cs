namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Students.Reports;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Constellation.Application.Domains.Students.Queries.GetCurrentStudentsWithCurrentOfferings;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public sealed class ClassAuditModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public ClassAuditModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<ClassAuditModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Students_Reports;
    [ViewData] public string PageTitle => "Student Class Audit";

    public List<StudentWithOfferingsResponse> Students { get; set; } = new();

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve class audit list of Students by user {User}", _currentUserService.UserName);

        Result<List<StudentWithOfferingsResponse>> studentRequest = await _mediator.Send(new GetCurrentStudentsWithCurrentOfferingsQuery());

        if (studentRequest.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(studentRequest.Error);

            _logger
                .ForContext(nameof(Error), studentRequest.Error, true)
                .Warning("Failed to retrieve class audit list of Students by user {User}", _currentUserService.UserName);

            return;
        }

        Students = studentRequest.Value
            .OrderBy(student => student.Grade)
            .ThenBy(student => student.Name.SortOrder)
            .ToList();
    }
}