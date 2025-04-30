namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Stocktake;

using Application.Common.PresentationModels;
using Application.Domains.AssetManagement.Stocktake.Queries.GetStocktakeEvent;
using Application.Models.Auth;
using Constellation.Application.Domains.AssetManagement.Stocktake.Commands.UpsertStocktakeEvent;
using Constellation.Application.Domains.AssetManagement.Stocktake.Models;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.CanManageAssets)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly IDateTimeProvider _dateTime;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public UpsertModel(
        ISender mediator,
        IDateTimeProvider dateTime,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _dateTime = dateTime;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<UpsertModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Stocktake_List;
    [ViewData] public string PageTitle => Id is null ? "New Stocktake Event" : "Edit Stocktake Event";

    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }

    [BindProperty]
    public string Name { get; set; }

    [BindProperty]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateOnly StartDate { get; set; }

    [BindProperty]
    [DataType(DataType.Date)]
    public DateOnly EndDate { get; set; }

    [BindProperty]
    public bool AcceptLateResponses { get; set; }


    public async Task OnGet()
    {
        StartDate = _dateTime.Today;
        EndDate = _dateTime.Today;

        if (!Id.HasValue)
            return;

        _logger
            .Information("Requested to retrieve Stocktake Event with Id {Id} for edit by user {User}", Id, _currentUserService.UserName);

        Result<StocktakeEventResponse> stocktake = await _mediator.Send(new GetStocktakeEventQuery(Id.Value));

        if (stocktake.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                stocktake.Error,
                _linkGenerator.GetPathByPage("/Equipment/Stocktake/Index", values: new { area = "Staff" }));

            _logger
                .ForContext(nameof(Error), stocktake.Error, true)
                .Warning("Failed to retrieve Stocktake Event with Id {Id} for edit by user {User}", Id, _currentUserService.UserName);
            
            return;
        }

        Name = stocktake.Value.Name;
        StartDate = DateOnly.FromDateTime(stocktake.Value.StartDate);
        EndDate = DateOnly.FromDateTime(stocktake.Value.EndDate);
        AcceptLateResponses = stocktake.Value.AcceptLateResponses;
    }

    public async Task<IActionResult> OnPost()
    {
        if (EndDate < StartDate)
            ModelState.AddModelError("EndDate", "Cannot create an event that ends before it starts!");

        if (EndDate < _dateTime.Today)
            ModelState.AddModelError("EndDate", "Cannot create an event that ends in the past!");

        if (!ModelState.IsValid)
            return Page();

        UpsertStocktakeEventCommand command = new(
            Id,
            Name,
            StartDate.ToDateTime(TimeOnly.MinValue),
            EndDate.ToDateTime(TimeOnly.MinValue),
            AcceptLateResponses);

        _logger
            .ForContext(nameof(UpsertStocktakeEventCommand), command, true)
            .Information("Requested to submit Stocktake Event by user {User}", _currentUserService.UserName);

        await _mediator.Send(command);

        return RedirectToPage("/Equipment/Stocktake/Index", new { area = "Staff" });
    }
}
