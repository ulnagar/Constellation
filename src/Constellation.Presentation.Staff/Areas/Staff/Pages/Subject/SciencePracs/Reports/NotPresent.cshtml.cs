namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.SciencePracs.Reports;

using Application.Domains.SciencePracs.Queries.GetRollsWithoutPresentStudents;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.Extensions;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[HasPermission(AuthPermission.Subjects_SciencePracs_Edit_Value)]
public class NotPresentModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public NotPresentModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForStaffPortal();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_SciencePracs_Reports;
    [ViewData] public string PageTitle => "Lesson Roll Reports";

    public List<NotPresentRollResponse> Rolls { get; set; } = new();

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve marked Lesson Rolls with no students present by user {User}", _currentUserService.UserName);

        Result<List<NotPresentRollResponse>> request = await _mediator.Send(new GetRollsWithoutPresentStudentsQuery());

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to retrieve marked Lesson Rolls with no students present by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(request.Error);

            return;
        }

        Rolls = request.Value.OrderByDescending(entry => entry.DueDate).ToList();
    }
}