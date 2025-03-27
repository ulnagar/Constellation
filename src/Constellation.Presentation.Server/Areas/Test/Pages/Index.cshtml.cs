namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Awards.Enums;
using BaseModels;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Students.GetCurrentStudentsAsDictionary;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Models.Students.Identifiers;
using Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Threading;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;

        _logger = logger;
    }

    [BindProperty]
    public List<StudentId> StudentIds { get; set; } = new();
    [BindProperty]
    public IssueAwardType AwardType { get; set; }

    public Dictionary<StudentId, string> StudentsList { get; set; } = new();

    public async Task OnGet()
    {
        Result<Dictionary<StudentId, string>> studentsRequest = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery());

        if (studentsRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), studentsRequest.Error, true)
                .Warning("Failed to retrieve list of Absences by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(studentsRequest.Error);

            return;
        }

        StudentsList = studentsRequest.Value;
    }

    public async Task<IActionResult> OnPost()
    {

        return RedirectToPage();
    }
}