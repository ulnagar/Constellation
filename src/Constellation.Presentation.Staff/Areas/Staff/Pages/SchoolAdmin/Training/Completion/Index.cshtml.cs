namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Training.Completion;

using Application.Common.PresentationModels;
using Application.Domains.Training.Models;
using Application.Domains.Training.Queries.GetListOfCompletionRecords;
using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Models.StaffMembers.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using Shared.Components.StaffTrainingReport;
using System.Collections.Generic;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.CanViewTrainingModuleContent)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IDateTimeProvider _dateTime;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator, 
        LinkGenerator linkGenerator,
        IDateTimeProvider dateTime,
        IAuthorizationService authorizationService,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _dateTime = dateTime;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
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
        AuthorizationResult authCheck = await _authorizationService.AuthorizeAsync(User, AuthPolicies.CanViewTrainingModuleContentDetails);

        if (!authCheck.Succeeded)
        {
            return RedirectToPage("/SchoolAdmin/Training/Staff/Index", new { area = "Staff", StaffId = staffId });
        }

        _logger.Information("Requested to retrieve list of Training Completion records by user {User}", _currentUserService.UserName);

        Result<List<CompletionRecordDto>> recordsRequest = await _mediator.Send(new GetListOfCompletionRecordsQuery(StaffId.Empty));

        if (recordsRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), recordsRequest.Error, true)
                .Warning("Failed to retrieve list of Training Completion records by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                recordsRequest.Error,
                _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Staff" }));

            return Page();
        }

        CompletionRecords = recordsRequest.Value;

        foreach (CompletionRecordDto record in CompletionRecords)
        {
            record.ExpiryCountdown = record.CalculateExpiry(_dateTime);
            record.Status = CompletionRecordDto.ExpiryStatus.Active;

            if (CompletionRecords.Any(s => s.ModuleId == record.ModuleId && s.StaffId == record.StaffId && s.CompletedDate > record.CompletedDate))
            {
                record.Status = CompletionRecordDto.ExpiryStatus.Superseded;
            }
        }

        CompletionRecords = Filter switch
        {
            FilterDto.Current => CompletionRecords.Where(record => record.Status == CompletionRecordDto.ExpiryStatus.Active).ToList(),
            FilterDto.All => CompletionRecords,
            FilterDto.Expiring => CompletionRecords.Where(record => record is { Status: CompletionRecordDto.ExpiryStatus.Active, ExpiryCountdown: < 31 }).ToList(),
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
