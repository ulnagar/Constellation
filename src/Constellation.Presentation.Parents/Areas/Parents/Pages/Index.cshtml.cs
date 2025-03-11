namespace Constellation.Presentation.Parents.Areas.Parents.Pages;

using Application.Interfaces.Configuration;
using Application.Models.Auth;
using Application.ThirdPartyConsent.DoesStudentHaveRequiredApplicationWithoutConsent;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Students.GetStudentsByParentEmail;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Models;
using Serilog;

[Authorize(Policy = AuthPolicies.IsParent)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly IOptions<ParentPortalConfiguration> _configuration;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        IOptions<ParentPortalConfiguration> configuration,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _configuration = configuration;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.ParentPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.Dashboard;

    public bool PendingApplications { get; set; }

    public async Task OnGet()
    {
        if (!_configuration.Value.ShowConsent)
            return;

        Result<List<StudentResponse>> studentsRequest = await _mediator.Send(new GetStudentsByParentEmailQuery(_currentUserService.EmailAddress));

        if (studentsRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(studentsRequest.Error);

            return;
        }

        foreach (StudentResponse student in studentsRequest.Value)
        {
            if (!student.ResidentialFamily)
                continue;

            Result<bool> hasPendingApplicationConsents = await _mediator.Send(new DoesStudentHaveRequiredApplicationWithoutConsentQuery(student.StudentId));

            if (hasPendingApplicationConsents.IsFailure)
            {
                ModalContent = new ErrorDisplay(studentsRequest.Error);

                return;
            }

            if (hasPendingApplicationConsents.Value)
            {
                PendingApplications = true;
                return;
            }
        }
    }
}
