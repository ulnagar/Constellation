namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Absences;

using Application.Absences.CreateAbsenceResponseFromSchool;
using Application.Common.PresentationModels;
using Application.Models.Auth;
using Constellation.Application.Absences.GetAbsenceDetailsForSchool;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class ExplainModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public ExplainModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger,
        IHttpContextAccessor httpContextAccessor, 
        IServiceScopeFactory serviceFactory) 
        : base(httpContextAccessor, serviceFactory)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<ExplainModel>()
            .ForContext("APPLICATION", "Schools Portal");
    }

    [ViewData] public string ActivePage => Models.ActivePage.Absences;

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(StrongIdBinder))]
    public AbsenceId AbsenceId { get; set; }

    public string StudentName { get; set; } = string.Empty;
    public DateOnly AbsenceDate { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string PeriodName { get; set; } = string.Empty;
    public string AbsenceTimeframe { get; set; } = string.Empty;

    [BindProperty] 
    public string Comment { get; set; }

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve absence data by user {user} with Id {absenceId}", _currentUserService.UserName, AbsenceId);

        Result<SchoolAbsenceDetailsResponse> absenceRequest = await _mediator.Send(new GetAbsenceDetailsForSchoolQuery(AbsenceId));

        if (!absenceRequest.IsSuccess)
        {
            ModalContent = new ErrorDisplay(
                absenceRequest.Error,
                _linkGenerator.GetPathByPage("/Absences/Index", values: new { area = "Schools" }));

            return;
        }

        StudentName = absenceRequest.Value.StudentName;
        AbsenceDate = DateOnly.FromDateTime(absenceRequest.Value.AbsenceDate);
        ClassName = absenceRequest.Value.ClassName;
        PeriodName = absenceRequest.Value.PeriodName;
        AbsenceTimeframe = absenceRequest.Value.AbsenceTimeframe;
    }

    public async Task<IActionResult> OnPost()
    {
        CreateAbsenceResponseFromSchoolCommand command = new(
            AbsenceId,
            Comment,
            _currentUserService.EmailAddress);

        _logger.Information("Requested to explain absence by user {user} with details {@absence}", _currentUserService.UserName, command);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            ModalContent = new ErrorDisplay(result.Error);

            return Page();
        }

        ModalContent = new FeedbackDisplay(
            "Absence Explanation",
            "Absence explanation has been forwarded to our Administration Team. Thank you!",
            "Ok",
            "btn-success",
            _linkGenerator.GetPathByPage("/Absences/Index", values: new { area = "Schools" }));

        return Page();
    }
}