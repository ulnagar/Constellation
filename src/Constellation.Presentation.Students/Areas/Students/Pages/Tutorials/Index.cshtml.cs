namespace Constellation.Presentation.Students.Areas.Students.Pages.Tutorials;

using Constellation.Core.Abstractions.Services;
using Core.Models.Timetables;
using Core.Models.Tutorials.Enums;
using Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;

public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StudentPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.Tutorials;

    public List<TutorialResponse> Tutorials { get; set; } = [];

    public async Task OnGet()
    {
    }

    public class TutorialResponse
    {
        public DateOnly SortDate { get; set; }
        public TutorialType Type { get; set; }
        public TutorialStatus Status { get; set; }
        public string Start { get; set; } = "Term 4, Week 1";
        public string End { get; set; } = "Term 4, Week 10";

        public List<Name> Teachers { get; set; }
        public List<Period> Sessions { get; set; }
    }
}