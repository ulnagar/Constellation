namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Stocktake;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.Stocktake.GetStocktakeEvent;
using Application.Stocktake.Models;
using Application.Stocktake.UpsertStocktakeEvent;
using Core.Abstractions.Clock;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.CanManageAssets)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly IDateTimeProvider _dateTime;
    private readonly LinkGenerator _linkGenerator;

    public UpsertModel(
        ISender mediator,
        IDateTimeProvider dateTime,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _dateTime = dateTime;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Stocktake_List;

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

        Result<StocktakeEventResponse> stocktake = await _mediator.Send(new GetStocktakeEventQuery(Id.Value));

        if (stocktake.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                stocktake.Error,
                _linkGenerator.GetPathByPage("/Equipment/Stocktake/Index", values: new { area = "Staff" }));

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

        await _mediator.Send(command);

        return RedirectToPage("/Equipment/Stocktake/Index", new { area = "Staff" });
    }
}
