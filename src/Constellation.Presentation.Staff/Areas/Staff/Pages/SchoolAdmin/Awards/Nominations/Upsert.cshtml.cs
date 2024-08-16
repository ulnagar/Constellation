namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Awards.Nominations;

using Application.Common.PresentationModels;
using Constellation.Application.Awards.CreateNominationPeriod;
using Constellation.Application.Awards.GetNominationPeriod;
using Constellation.Application.Models.Auth;
using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;

[Authorize(Policy = AuthPolicies.CanAddAwards)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public UpsertModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<UpsertModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Awards_Nominations;
    [ViewData] public string PageTitle { get; set; } = "New Award Period";

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(ConstructorBinder))]
    public AwardNominationPeriodId Id { get; set; } = AwardNominationPeriodId.Empty;

    [BindProperty]
    public string Name { get; set; } = string.Empty;
    [BindProperty]
    public DateOnly LockoutDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [BindProperty]
    public List<Grade> Grades { get; set; } = new();

    public async Task OnGet(CancellationToken cancellationToken = default)
    {
        if (Id != AwardNominationPeriodId.Empty)
        {
            _logger.Information("Requested to retrieve Award Nomination Period with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

            Result<NominationPeriodDetailResponse> details = await _mediator.Send(new GetNominationPeriodRequest(Id), cancellationToken);

            if (details.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), details.Error, true)
                    .Warning("Failed to retrieve Award Nomination Period with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

                ModalContent = new ErrorDisplay(
                    details.Error,
                    _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = Id }));

                return;
            }

            Name = details.Value.Name;
            LockoutDate = details.Value.LockoutDate;
            Grades = details.Value.IncludedGrades;

            PageTitle = $"Award Period - {Name}";
        }
    }

    public async Task<IActionResult> OnPost(CancellationToken cancellationToken = default)
    {
        if (Id == AwardNominationPeriodId.Empty && !Grades.Any())
            ModelState.AddModelError("Grades", "You must select at least one grade");

        if (!ModelState.IsValid) 
            return Page();

        if (Id != AwardNominationPeriodId.Empty)
        {
            UpdateNominationPeriodCommand command = new(Id, Name, LockoutDate);

            _logger
                .ForContext(nameof(UpdateNominationPeriodCommand), command, true)
                .Information("Requested to update Award Nomination Period by user {User}", _currentUserService.UserName);
            
            Result editRequest = await _mediator.Send(command, cancellationToken);

            if (editRequest.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), editRequest.Error, true)
                    .Warning("Failed to update Award Nomination Period by user {User}", _currentUserService.UserName);

                ModalContent = new ErrorDisplay(editRequest.Error);

                PageTitle = $"Award Period - {Name}";
                
                return Page();
            }
        }
        else
        {
            CreateNominationPeriodCommand command = new(Name, LockoutDate, Grades);

            _logger
                .ForContext(nameof(CreateNominationPeriodCommand), command, true)
                .Information("Requested to create new Award Nomination Period by user {User}", _currentUserService.UserName);

            Result<AwardNominationPeriodId> request = await _mediator.Send(command, cancellationToken);

            if (request.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), request.Error, true)
                    .Warning("Failed to create new Award Nomination Period by user {User}", _currentUserService.UserName);
                
                ModalContent = new ErrorDisplay(request.Error);

                return Page();
            }

            Id = request.Value;
        }
        
        return RedirectToPage("/SchoolAdmin/Awards/Nominations/Details", new { area = "Staff", PeriodId = Id.Value });
    }
}
