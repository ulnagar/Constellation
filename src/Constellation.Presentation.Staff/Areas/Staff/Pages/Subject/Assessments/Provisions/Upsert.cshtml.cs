namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Assessments.Provisions;

using Application.Common.PresentationModels;
using Application.Domains.Assessments.Provisions.Commands.CreateNewAssessmentProvision;
using Application.Domains.Assessments.Provisions.Commands.UpdateAssessmentProvision;
using Application.Domains.Assessments.Provisions.Models;
using Application.Domains.Assessments.Provisions.Queries.GetAssessmentProvisionById;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Core.Abstractions.Services;
using Core.Models.Assessments.Identifiers;
using Core.Models.Assessments.ValueObjects;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Extensions;
using Presentation.Shared.Helpers.Logging;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;

[HasPermission(AuthPermission.Subjects_AssessmentsProvisions_Edit_Value)]
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
            .ForStaffPortal();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Assessments_Provisions;
    [ViewData] public string PageTitle => "Update Provision";

    [BindProperty(SupportsGet = true)]
    public ProvisionId Id { get; set; } = ProvisionId.Empty;

    [BindProperty]
    [ModelBinder(typeof(FromValueBinder))]
    public ProvisionCode Code { get; set; }

    [BindProperty]
    public string Description { get; set; }

    public async Task OnGet()
    {
        if (Id == ProvisionId.Empty)
            return;

        Result<AssessmentProvisionResponse> provision = await _mediator.Send(new GetAssessmentProvisionByIdQuery(Id));

        if (provision.IsFailure)
        {
            _logger
                .ForContext(nameof(ProvisionId), Id, true)
                .ForContext(nameof(Error), provision.Error, true)
                .Warning("Failed to retrieve Assessment Provision for editing by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                provision.Error,
                _linkGenerator.GetPathByPage("/Subject/Assessments/Provisions/Index", values: new { area = "Staff" }));

            return;
        }

        Code = provision.Value.Code;
        Description = provision.Value.Description;
    }

    public async Task<IActionResult> OnPost()
    {
        if (Id == ProvisionId.Empty)
        {
            CreateNewAssessmentProvisionCommand command = new CreateNewAssessmentProvisionCommand(Code, Description);

            _logger
                .ForContext(nameof(CreateNewAssessmentProvisionCommand), command, true)
                .Information("Requested to create new Assessment Provision by user {User}", _currentUserService.UserName);

            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                _logger
                    .ForContext(nameof(CreateNewAssessmentProvisionCommand), command, true)
                    .ForContext(nameof(Error), result.Error, true)
                    .Warning("Failed to create new Assessment Provision by user {User}", _currentUserService.UserName);

                ModalContent = ErrorDisplay.Create(result.Error);

                return Page();
            }
        }
        else
        {
            UpdateAssessmentProvisionCommand command = new UpdateAssessmentProvisionCommand(Id, Code, Description);

            _logger
                .ForContext(nameof(UpdateAssessmentProvisionCommand), command, true)
                .Information("Requested to update Assessment Provision by user {User}", _currentUserService.UserName);

            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                _logger
                    .ForContext(nameof(UpdateAssessmentProvisionCommand), command, true)
                    .ForContext(nameof(Error), result.Error, true)
                    .Warning("Failed to update Assessment Provision by user {User}", _currentUserService.UserName);

                ModalContent = ErrorDisplay.Create(result.Error);

                return Page();
            }
        }

        return RedirectToPage("/Subject/Assessments/Provisions/Index", new { area = "Staff" });
    }
}