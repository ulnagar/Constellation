﻿namespace Constellation.Presentation.Server.ViewComponents;

using Application.Offerings.GetCurrentOfferingsForTeacher;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pages.Shared.Components.ClassListNavBar;

public class ClassListNavBarViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public ClassListNavBarViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        ClassListNavBarViewModel viewModel = new();

        string username = User.Identity?.Name;

        if (username is null)
            return View(viewModel);

        Result<List<TeacherOfferingResponse>> query = await _mediator.Send(new GetCurrentOfferingsForTeacherQuery(null, username));

        if (query.IsFailure)
            return View(viewModel);

        foreach (TeacherOfferingResponse entry in query.Value)
            viewModel.Classes.Add(entry.OfferingName, entry.OfferingId);

        return View(viewModel);
    }
}