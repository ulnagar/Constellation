namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.MandatoryTraining.Completion;

using Constellation.Application.Features.MandatoryTraining.Commands;
using Constellation.Application.Features.MandatoryTraining.Models;
using Constellation.Application.Features.MandatoryTraining.Queries;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Pages.Shared.Components.StaffTrainingReport;
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

    public IndexModel(IMediator mediator, IAuthorizationService authorizationService)
    {
        _mediator = mediator;
        _authorizationService = authorizationService;
    }

    public List<CompletionRecordDto> CompletionRecords { get; set; } = new();
    
    [BindProperty(SupportsGet = true)]
    public FilterDto Filter { get; set; }

    [BindProperty]
    public StaffTrainingReportSelection Report { get; set; } 

    public async Task<IActionResult> OnGet()
    {
        await GetClasses(_mediator);

        // If user does not have details view permissions, only show their own records
        if (User.HasClaim(claim => claim.Type == AuthClaimType.Permission && claim.Value == AuthPermissions.MandatoryTrainingDetailsView))
        {
            CompletionRecords = await _mediator.Send(new GetListOfCompletionRecordsQuery());
        }
        else
        {
            var staffId = User.FindFirst(AuthClaimType.StaffEmployeeId).Value;
            return RedirectToPage("/MandatoryTraining/Staff/Index", new { StaffId = staffId });
        }

        foreach (var record in CompletionRecords)
        {
            record.ExpiryCountdown = record.CalculateExpiry();
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
        var isAuthorised = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanRunTrainingModuleReports);

        //TODO: Show error explaining why this did not work!
        if (!isAuthorised.Succeeded)
            return Page();

        if (string.IsNullOrWhiteSpace(Report.StaffId))
        {
            return Page();
        }

        ReportDto report;

        if (Report.IncludeCertificates)
        {
            report = await _mediator.Send(new GenerateStaffReportWithCertificatesCommand(Report.StaffId));
        } 
        else
        {
            report = await _mediator.Send(new GenerateStaffReportCommand(Report.StaffId));
        }

        return File(report.FileData, report.FileType, report.FileName);
    }

    public enum FilterDto
    {
        All,
        Current,
        Expiring
    }
}
