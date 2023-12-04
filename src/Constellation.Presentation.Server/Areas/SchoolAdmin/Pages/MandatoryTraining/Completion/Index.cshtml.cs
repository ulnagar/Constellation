namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.MandatoryTraining.Completion;

using Constellation.Application.MandatoryTraining.GenerateStaffReport;
using Constellation.Application.MandatoryTraining.GetListOfCompletionRecords;
using Constellation.Application.MandatoryTraining.Models;
using Constellation.Application.Models.Auth;
using Constellation.Core.Errors;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Pages.Shared.Components.StaffTrainingReport;
using Core.Abstractions.Clock;
using Core.Models.Students.Errors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.CanViewTrainingModuleContent)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly IAuthorizationService _authorizationService;
    private readonly LinkGenerator _linkGenerator;
    private readonly IDateTimeProvider _dateTime;

    public IndexModel(
        IMediator mediator, 
        IAuthorizationService authorizationService,
        LinkGenerator linkGenerator,
        IDateTimeProvider dateTime)
    {
        _mediator = mediator;
        _authorizationService = authorizationService;
        _linkGenerator = linkGenerator;
        _dateTime = dateTime;
    }

    public List<CompletionRecordDto> CompletionRecords { get; set; } = new();
    
    [BindProperty(SupportsGet = true)]
    public FilterDto Filter { get; set; }

    [BindProperty]
    public StaffTrainingReportSelection Report { get; set; } 

    public async Task<IActionResult> OnGet()
    {

        ViewData["ActivePage"] = "Completions";
        ViewData["StaffId"] = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        await GetClasses(_mediator);

        // If user does not have details view permissions, only show their own records
        if (User.HasClaim(claim => claim.Type == AuthClaimType.Permission && claim.Value == AuthPermissions.MandatoryTrainingDetailsView))
        {
            var recordsRequest = await _mediator.Send(new GetListOfCompletionRecordsQuery(null));

            if (recordsRequest.IsFailure)
            {
                Error = new ErrorDisplay
                {
                    Error = recordsRequest.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Home" })
                };

                return Page();
            }

            CompletionRecords = recordsRequest.Value;
        }
        else
        {
            var staffId = User.FindFirst(AuthClaimType.StaffEmployeeId).Value;
            return RedirectToPage("/MandatoryTraining/Staff/Index", new { StaffId = staffId });
        }

        foreach (var record in CompletionRecords)
        {
            record.ExpiryCountdown = record.CalculateExpiry(_dateTime);
            record.Status = CompletionRecordDto.ExpiryStatus.Active;

            if (CompletionRecords.Any(s => s.ModuleId == record.ModuleId && s.StaffId == record.StaffId && s.CompletedDate > record.CompletedDate))
            {
                record.Status = CompletionRecordDto.ExpiryStatus.Superceded;
            }
        }

        CompletionRecords = Filter switch
        {
            FilterDto.Current => CompletionRecords.Where(record => record.Status == CompletionRecordDto.ExpiryStatus.Active).ToList(),
            FilterDto.All => CompletionRecords,
            FilterDto.Expiring => CompletionRecords.Where(record => record.Status == CompletionRecordDto.ExpiryStatus.Active && record.ExpiryCountdown < 31).ToList(),
            _ => CompletionRecords
        };

        CompletionRecords = CompletionRecords.OrderByDescending(record => record.CompletedDate).ThenBy(record => record.StaffLastName).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostStaffReport()
    {

        ViewData["ActivePage"] = "Completions";
        ViewData["StaffId"] = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        var isAuthorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanRunTrainingModuleReports);

        if (!isAuthorised.Succeeded)
        {
            Error = new ErrorDisplay
            {
                Error = DomainErrors.Permissions.Unauthorised,
                RedirectPath = _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Home" })
            };

            return Page();
        }

        if (string.IsNullOrWhiteSpace(Report.StaffId))
        {
            Error = new ErrorDisplay
            {
                Error = StudentErrors.NotFound(""),
                RedirectPath = _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Home" })
            };

            return Page();
        }

        var reportRequest = await _mediator.Send(new GenerateStaffReportCommand(Report.StaffId, Report.IncludeCertificates));

        if (reportRequest.IsFailure)
        {
            Error = new ErrorDisplay
            {
                Error = reportRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Home" })
            };

            return Page();
        }

        return File(reportRequest.Value.FileData, reportRequest.Value.FileType, reportRequest.Value.FileName);
    }

    public enum FilterDto
    {
        All,
        Current,
        Expiring
    }
}
