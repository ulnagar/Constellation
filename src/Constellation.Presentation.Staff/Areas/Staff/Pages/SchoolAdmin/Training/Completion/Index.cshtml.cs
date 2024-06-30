namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Training.Completion;

using Application.Training.GetListOfCompletionRecords;
using Application.Training.Models;
using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Pages.Shared.Components.StaffTrainingReport;
using System.Collections.Generic;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.CanViewTrainingModuleContent)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IDateTimeProvider _dateTime;

    public IndexModel(
        IMediator mediator, 
        LinkGenerator linkGenerator,
        IDateTimeProvider dateTime)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _dateTime = dateTime;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Training_Completions;
    [ViewData] public string PageTitle => "Training Completions";
    
    public List<CompletionRecordDto> CompletionRecords { get; set; } = new();
    
    [BindProperty(SupportsGet = true)]
    public FilterDto Filter { get; set; }

    [BindProperty]
    public StaffTrainingReportSelection Report { get; set; }
    
    public async Task<IActionResult> OnGet()
    {
        string? staffId = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;
        
        // If user does not have details view permissions, only show their own records
        if (User.HasClaim(claim => claim is { Type: AuthClaimType.Permission, Value: AuthPermissions.MandatoryTrainingDetailsView }))
        {
            Result<List<CompletionRecordDto>> recordsRequest = await _mediator.Send(new GetListOfCompletionRecordsQuery(null));

            if (recordsRequest.IsFailure)
            {
                Error = new()
                {
                    Error = recordsRequest.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Staff" })
                };

                return Page();
            }

            CompletionRecords = recordsRequest.Value;
        }
        else
        {
            return RedirectToPage("/SchoolAdmin/Training/Staff/Index", new { area = "Staff", StaffId = staffId });
        }

        foreach (CompletionRecordDto record in CompletionRecords)
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

    public enum FilterDto
    {
        All,
        Current,
        Expiring
    }
}
