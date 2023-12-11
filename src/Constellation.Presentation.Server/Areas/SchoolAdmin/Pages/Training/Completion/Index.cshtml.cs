namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Training.Completion;

using Application.Training.Models;
using Constellation.Application.Models.Auth;
using Constellation.Application.Training.Modules.GenerateStaffReport;
using Constellation.Application.Training.Modules.GetListOfCompletionRecords;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Errors;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Training;
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

    [ViewData] public string ActivePage { get; set; } = TrainingPages.Completions;
    [ViewData] public string StaffId { get; set; }

    public async Task<IActionResult> OnGet()
    { 
        await GetClasses(_mediator);

        StaffId = User.FindFirst(AuthClaimType.StaffEmployeeId)?.Value;
        
        // If user does not have details view permissions, only show their own records
        if (User.HasClaim(claim => claim.Type == AuthClaimType.Permission && claim.Value == AuthPermissions.MandatoryTrainingDetailsView))
        {
            Result<List<CompletionRecordDto>> recordsRequest = await _mediator.Send(new GetListOfCompletionRecordsQuery(null));

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
            return RedirectToPage("/Training/Staff/Index", new { StaffId = StaffId });
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
