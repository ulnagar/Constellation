﻿namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.TutorialStudentEnrolment;

using Constellation.Application.Students.GetCurrentStudentsAsDictionary;
using Core.Models.Students.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class TutorialStudentEnrolmentViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public TutorialStudentEnrolmentViewComponent(ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        TutorialStudentEnrolmentSelection viewModel = new();
        Result<Dictionary<StudentId, string>> result = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery());

        if (result.IsFailure)
        {
            // How? This method does not return failure
        }
        else
        {
            viewModel.StudentList = result.Value;
        }

        return View(viewModel);
    }
}