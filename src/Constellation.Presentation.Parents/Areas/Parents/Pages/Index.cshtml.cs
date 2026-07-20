namespace Constellation.Presentation.Parents.Areas.Parents.Pages;

using Application.Domains.ThirdPartyConsent.Queries.DoesStudentHaveRequiredApplicationWithoutConsent;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.Students.Queries.GetStudentsByParentEmail;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Models;
using Presentation.Shared.Extensions;
using Serilog;

[HasPermission(AuthPermission.ParentPortal_View_Value)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForParentPortal();
    }

    [ViewData] public string ActivePage => Models.ActivePage.Dashboard;

    public bool PendingApplications { get; set; }

    public async Task OnGet()
    {
        Result<List<StudentResponse>> studentsRequest = await _mediator.Send(new GetStudentsByParentEmailQuery(_currentUserService.EmailAddress));

        if (studentsRequest.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(studentsRequest.Error);

            return;
        }

        foreach (StudentResponse student in studentsRequest.Value)
        {
            if (!student.ResidentialFamily)
                continue;

            Result<bool> hasPendingApplicationConsents = await _mediator.Send(new DoesStudentHaveRequiredApplicationWithoutConsentQuery(student.StudentId));

            if (hasPendingApplicationConsents.IsFailure)
            {
                ModalContent = ErrorDisplay.Create(studentsRequest.Error);

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
